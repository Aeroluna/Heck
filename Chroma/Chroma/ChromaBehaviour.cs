using Chroma.Events;
using Chroma.HarmonyPatches;
using Chroma.Settings;
using Chroma.Utils;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chroma
{
    public class ChromaBehaviour : MonoBehaviour
    {
        private static bool isLoadingSong = false;

        public static bool IsLoadingSong
        {
            get { return isLoadingSong; }
            set
            {
                if (value)
                {
                    LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ResetRandom();
                    ParticleSystemEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ResetRandom();
                }
                isLoadingSong = value;
            }
        }

        public static ChromaBehaviour Instance { get; private set; } = null;

        public static void ClearInstance()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                Instance = null;
            }
        }

        internal static ChromaBehaviour CreateNewInstance()
        {
            IsLoadingSong = true;
            ChromaLogger.Log("ChromaBehaviour attempting creation.", ChromaLogger.Level.DEBUG);
            ClearInstance();

            //if (SceneUtils.IsTargetGameScene(SceneManager.GetActiveScene().buildIndex)) {
            GameObject instanceObject = new GameObject("ChromaBehaviour");
            ChromaBehaviour behaviour = instanceObject.AddComponent<ChromaBehaviour>();
            Instance = behaviour;
            ChromaLogger.Log("ChromaBehaviour instantiated.", ChromaLogger.Level.DEBUG);
            return behaviour;
            /*} else {
                ChromaLogger.Log("Invalid scene index.");
                return null;
            }*/
        }

        public PlayerController PlayerController { get; private set; }

        /// <summary>
        /// Called when CB is instantiated, used to attach extensions.
        /// Extensions will be useless if not tracked.  Call ChromaBehaviour.AttachExtension to solve this.
        /// </summary>
        public static event ChromaBehaviourInstantiatedDelegate ChromaBehaviourInstantiated;

        public delegate void ChromaBehaviourInstantiatedDelegate(ChromaBehaviour behaviour);

        public event ChromaHandleNoteWasCut ChromaHandleNoteWasCutEvent;

        public delegate void ChromaHandleNoteWasCut(BeatmapObjectSpawnController noteSpawnController, INoteController noteController, NoteCutInfo noteCutInfo);

        public event ChromaHandleNoteWasMissed ChromaHandleNoteWasMissedEvent;

        public delegate void ChromaHandleNoteWasMissed(BeatmapObjectSpawnController noteSpawnController, INoteController noteController);

        public event ChromaHandleBarrierSpawned ChromaHandleBarrierSpawnedEvent;

        public delegate void ChromaHandleBarrierSpawned(ref StretchableObstacle stretchableObstacle, ref BeatmapObjectSpawnController obstacleSpawnController, ref ObstacleController obstacleController);

        public event ChromaHandleComboChange ChromaHandleComboChangeEvent;

        public delegate void ChromaHandleComboChange(int newCombo);

        //public event ChromaHandleNoteScaling ChromaHandleNoteScalingEvent;
        //public delegate void ChromaHandleNoteScaling(int noteID, NoteType noteType, ref float tScale);

        private BeatmapObjectSpawnController beatmapObjectSpawnController;
        private ScoreController scoreController;

        public static float songBPM = 120f;
        public static AudioTimeSyncController ATSC;
        public static bool LightingRegistered;

        internal List<IChromaBehaviourExtension> extensions = new List<IChromaBehaviourExtension>();

        /// <summary>
        /// Attaches an IChromaBehaviourExtension.
        /// Methods on IChromaBehaviourExtension will not be called unless attached.
        /// </summary>
        /// <param name="extension">The extension to attach.</param>
        public void AttachExtension(IChromaBehaviourExtension extension)
        {
            extensions.Add(extension);
        }

        private void OnDestroy()
        {
            ChromaLogger.Log("ChromaBehaviour destroyed.", ChromaLogger.Level.DEBUG);
            StopAllCoroutines();

            if (beatmapObjectSpawnController != null)
            {
                beatmapObjectSpawnController.obstacleDiStartMovementEvent -= HandleObstacleDidStartMovementEvent;
                beatmapObjectSpawnController.noteWasCutEvent -= HandleNoteWasCutEvent;
                beatmapObjectSpawnController.noteWasMissedEvent -= HandleNoteWasMissedEvent;
            }
            if (scoreController != null) scoreController.comboDidChangeEvent -= ComboChangedEvent;
        }

        private void Start()
        {
            StartCoroutine(DelayedStart());
        }

        private IEnumerator DelayedStart()
        {
            yield return new WaitForSeconds(0f);
            LightingRegistered = ChromaUtils.CheckLightingEventRequirement();
            ChromaBehaviourInstantiated?.Invoke(this);
            beatmapObjectSpawnController = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First();
            if (beatmapObjectSpawnController != null)
            {
                songBPM = beatmapObjectSpawnController.GetPrivateField<float>("_beatsPerMinute");
                ChromaLogger.Log("BPM Found : " + songBPM);
                ATSC = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().First();
            }
            BeatmapObjectCallbackController coreSetup = GetBeatmapObjectCallbackController();
            if (coreSetup != null)
            {
                ChromaLogger.Log("Found BOCC properly!", ChromaLogger.Level.DEBUG);
                try
                {
                    GCSSFound(SceneManager.GetActiveScene(), coreSetup);
                }
                catch (Exception e)
                {
                    ChromaLogger.Log(e);
                }
            }

            if (beatmapObjectSpawnController != null)
            {
                beatmapObjectSpawnController.obstacleDiStartMovementEvent += HandleObstacleDidStartMovementEvent;
                beatmapObjectSpawnController.noteWasCutEvent += HandleNoteWasCutEvent;
                beatmapObjectSpawnController.noteWasMissedEvent += HandleNoteWasMissedEvent;
            }

            scoreController = FindObjectsOfType<ScoreController>().FirstOrDefault();
            if (scoreController != null) scoreController.comboDidChangeEvent += ComboChangedEvent;

            Saber[] sabers = FindObjectsOfType<Saber>();
            if (sabers != null)
            {
                Extensions.SaberColourizer.InitializeSabers(sabers);
            }

            VFX.TechnicolourController.InitializeGradients();

            IsLoadingSong = false;
        }

        private BeatmapObjectCallbackController GetBeatmapObjectCallbackController()
        {
            BeatmapObjectCallbackController s = FindObjectOfType<BeatmapObjectCallbackController>();
            if (s == null)
            {
                s = Resources.FindObjectsOfTypeAll<BeatmapObjectCallbackController>().FirstOrDefault();
            }
            return s;
        }

        private void GCSSFound(Scene scene, BeatmapObjectCallbackController gcss)
        {
            ChromaLogger.Log("Found BOCC!", ChromaLogger.Level.DEBUG);

            PlayerController = FindObjectOfType<PlayerController>();
            if (PlayerController == null) ChromaLogger.Log("Player Controller not found!", ChromaLogger.Level.WARNING);

            if (gcss == null)
            {
                ChromaLogger.Log("Failed to obtain MainGameSceneSetup", ChromaLogger.Level.WARNING);
                return;
            }

            BS_Utils.Gameplay.LevelData levelData = BS_Utils.Plugin.LevelData;
            if (!levelData.IsSet)
            {
                ChromaLogger.Log("BS_Utils LevelData is not set", ChromaLogger.Level.WARNING);
                return;
            }
            PlayerSpecificSettings playerSettings = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.playerSpecificSettings;

            //Map

            BeatmapData _beatmapData = gcss.GetPrivateField<BeatmapData>("_beatmapData");
            if (_beatmapData == null) ChromaLogger.Log("{XXX} : NULL BEATMAP DATA", ChromaLogger.Level.ERROR);
            BeatmapData beatmapData = CreateTransformedData(_beatmapData);
            if (beatmapData != null) gcss.SetPrivateField("_beatmapData", beatmapData);

            foreach (IChromaBehaviourExtension extension in extensions) extension.PostInitialization(songBPM, beatmapData, playerSettings, scoreController);

            ColourManager.RefreshLights();

            if (ChromaConfig.LightshowModifier)
            {
                foreach (Saber saber in FindObjectsOfType<Saber>())
                {
                    saber.gameObject.SetActive(false);
                }

                if (ChromaConfig.PlayersPlace) GameObject.Find("PlayersPlace")?.SetActive(false);
                if (ChromaConfig.Spectrograms) GameObject.Find("Spectrograms")?.SetActive(false);
                if (ChromaConfig.BackColumns) GameObject.Find("BackColumns")?.SetActive(false);
                if (ChromaConfig.Buildings) GameObject.Find("Buildings")?.SetActive(false);
            }

            // CustomJSONData Custom Events
            Dictionary<string, List<CustomEventData>> _customEventData = ((CustomBeatmapData)_beatmapData).customEventData;
            foreach (KeyValuePair<string, List<CustomEventData>> n in _customEventData)
            {
                switch (n.Key)
                {
                    case "_obstacleColor":
                        ChromaObstacleColourEvent.Activate(n.Value);
                        break;

                    case "_noteColor":
                        if (ChromaConfig.NoteColourEventsEnabled) ChromaNoteColourEvent.Activate(n.Value);
                        break;

                    case "_bombColor":
                        ChromaBombColourEvent.Activate(n.Value);
                        break;

                    case "_lightRGB":
                        ChromaLightColourEvent.Activate(n.Value);
                        break;
                }
            }

            // SimpleCustomEvents subscriptions
            CustomEvents.CustomEventCallbackController cecc = gcss.GetComponentInParent<CustomEvents.CustomEventCallbackController>();
            if (LightingRegistered)
            {
                cecc.AddCustomEventCallback(ChromaGradientEvent.Callback, "_lightGradient", 0);
            }
        }

        public static BeatmapData CreateTransformedData(BeatmapData beatmapData)
        {
            ColourManager.TechnicolourLightsForceDisabled = false;
            ColourManager.TechnicolourBlocksForceDisabled = false;
            ColourManager.TechnicolourBarriersForceDisabled = false;
            ColourManager.TechnicolourBombsForceDisabled = false;

            if (beatmapData == null) ChromaLogger.Log("Null beatmapData", ChromaLogger.Level.ERROR);

            if (ChromaConfig.LightshowModifier)
            {
                foreach (BeatmapLineData b in beatmapData.beatmapLinesData)
                {
                    b.beatmapObjectsData = b.beatmapObjectsData.Where((source, index) => b.beatmapObjectsData[index].beatmapObjectType != BeatmapObjectType.Note).ToArray();
                }
                BS_Utils.Gameplay.ScoreSubmission.DisableSubmission("Chroma");
            }

            /*
             * LIGHTING EVENTS
             */

            if (ChromaConfig.CustomColourEventsEnabled)
            {
                BeatmapEventData[] bevData = beatmapData.beatmapEventData;
                for (int i = bevData.Length - 1; i >= 0; i--)
                {
                    if (LightingRegistered && bevData[i] is CustomBeatmapEventData customData)
                    {
                        dynamic dynData = customData.customData;
                        if (Trees.at(dynData, "_color") != null) ColourManager.TechnicolourLightsForceDisabled = true;
                        continue;
                    }
                    if (bevData[i].value >= ColourManager.RGB_INT_OFFSET) ColourManager.TechnicolourLightsForceDisabled = true;
                }

                BeatmapLineData[] bData = beatmapData.beatmapLinesData;
                foreach (BeatmapLineData b in bData)
                {
                    foreach (BeatmapObjectData beatmapObjectsData in b.beatmapObjectsData)
                    {
                        if (LightingRegistered)
                        {
                            if (beatmapObjectsData is CustomNoteData customNoteData)
                            {
                                dynamic dynData = customNoteData.customData;
                                if (Trees.at(dynData, "_color") != null)
                                {
                                    if (customNoteData.noteType == NoteType.Bomb) ColourManager.TechnicolourBombsForceDisabled = true;
                                    else ColourManager.TechnicolourBlocksForceDisabled = ChromaConfig.NoteColourEventsEnabled;
                                }
                            }
                            else if (beatmapObjectsData is CustomObstacleData customObstacleData)
                            {
                                dynamic dynData = customObstacleData.customData;
                                if (Trees.at(dynData, "_color") != null) ColourManager.TechnicolourBarriersForceDisabled = true;
                            }
                        }
                    }
                }
            }

            return beatmapData;
        }

        private void HandleObstacleDidStartMovementEvent(BeatmapObjectSpawnController obstacleSpawnController, ObstacleController obstacleController)
        {
            StretchableObstacle stretchableObstacle = ReflectionUtil.GetPrivateField<StretchableObstacle>(obstacleController, "_stretchableObstacle");
            ChromaHandleBarrierSpawnedEvent?.Invoke(ref stretchableObstacle, ref obstacleSpawnController, ref obstacleController);
        }

        private void HandleNoteWasCutEvent(BeatmapObjectSpawnController noteSpawnController, INoteController noteController, NoteCutInfo noteCutInfo)
        {
            ChromaHandleNoteWasCutEvent?.Invoke(noteSpawnController, noteController, noteCutInfo);
        }

        private void HandleNoteWasMissedEvent(BeatmapObjectSpawnController noteSpawnController, INoteController noteController)
        {
            ChromaHandleNoteWasMissedEvent?.Invoke(noteSpawnController, noteController);
        }

        private void ComboChangedEvent(int newCombo)
        {
            ChromaHandleComboChangeEvent?.Invoke(newCombo);
        }
    }
}