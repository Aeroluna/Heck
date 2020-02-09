using Chroma.Beatmap;
using Chroma.HarmonyPatches;
using Chroma.Settings;
using Chroma.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPA.Utilities;
using Chroma.Beatmap.Events;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;

namespace Chroma {

    public class ChromaBehaviour : MonoBehaviour {

        private static bool isLoadingSong = false;
        public static bool IsLoadingSong {
            get { return isLoadingSong; }
            set {
                if (value)
                {
                    LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ResetRandom();
                    ParticleSystemEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ResetRandom();
                }
                isLoadingSong = value;
            }
        }

        private static ChromaBehaviour _instance = null;

        public static ChromaBehaviour Instance {
            get { return _instance; }
        }

        public static void ClearInstance() {
            if (_instance != null) {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }

        internal static ChromaBehaviour CreateNewInstance() {
            IsLoadingSong = true;
            ChromaLogger.Log("ChromaBehaviour attempting creation.", ChromaLogger.Level.DEBUG);
            ClearInstance();

            //if (SceneUtils.IsTargetGameScene(SceneManager.GetActiveScene().buildIndex)) {
                GameObject instanceObject = new GameObject("ChromaBehaviour");
                ChromaBehaviour behaviour = instanceObject.AddComponent<ChromaBehaviour>();
                _instance = behaviour;
                ChromaLogger.Log("ChromaBehaviour instantiated.", ChromaLogger.Level.DEBUG);
                return behaviour;
            /*} else {
                ChromaLogger.Log("Invalid scene index.");
                return null;
            }*/
        }

        private PlayerController _playerController;

        public PlayerController PlayerController {
            get { return _playerController; }
        }

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

        BeatmapObjectSpawnController beatmapObjectSpawnController;
        ScoreController scoreController;

        public static float songBPM = 120f;
        public static AudioTimeSyncController ATSC;
        public static bool LightingRegistered;

        internal List<IChromaBehaviourExtension> extensions = new List<IChromaBehaviourExtension>();

        /// <summary>
        /// Attaches an IChromaBehaviourExtension.
        /// Methods on IChromaBehaviourExtension will not be called unless attached.
        /// </summary>
        /// <param name="extension">The extension to attach.</param>
        public void AttachExtension(IChromaBehaviourExtension extension) {
            extensions.Add(extension);
        }

        void OnDestroy() {
            ChromaLogger.Log("ChromaBehaviour destroyed.", ChromaLogger.Level.DEBUG);
            StopAllCoroutines();

            if (beatmapObjectSpawnController != null) {
                beatmapObjectSpawnController.obstacleDiStartMovementEvent -= HandleObstacleDidStartMovementEvent;
                beatmapObjectSpawnController.noteWasCutEvent -= HandleNoteWasCutEvent;
                beatmapObjectSpawnController.noteWasMissedEvent -= HandleNoteWasMissedEvent;
            }
            if (scoreController != null) scoreController.comboDidChangeEvent -= ComboChangedEvent;

            ObstacleControllerInit.defaultObstacleColour = null;
            ChromaObstacleColourEvent.CustomObstacleColours.Clear();
            ChromaBombColourEvent.CustomBombColours.Clear();
            ChromaLightColourEvent.CustomLightColours.Clear();
            ChromaGradientEvent.CustomGradients.Clear();

            ChromaGradientEvent.Clear();
            VFX.TechnicolourController.Clear();

            ColourManager.LightSwitchs = null;

            Beatmap.ChromaEvents.MayhemEvent.manager = null;
        }

        void Start() {
            StartCoroutine(DelayedStart());
        }

        private IEnumerator DelayedStart() {
            yield return new WaitForSeconds(0f);
            LightingRegistered = ChromaUtils.CheckLightingEventRequirement();
            ChromaBehaviourInstantiated?.Invoke(this);
            beatmapObjectSpawnController = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First();
            if (beatmapObjectSpawnController != null) {
                songBPM = beatmapObjectSpawnController.GetPrivateField<float>("_beatsPerMinute");
                ChromaLogger.Log("BPM Found : " + songBPM);
                ATSC = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().First();
            }
            BeatmapObjectCallbackController coreSetup = GetBeatmapObjectCallbackController();
            if (coreSetup != null) {
                ChromaLogger.Log("Found BOCC properly!", ChromaLogger.Level.DEBUG);
                try {
                    GCSSFound(SceneManager.GetActiveScene(), coreSetup);
                } catch (Exception e) {
                    ChromaLogger.Log(e);
                }
            }

            if (beatmapObjectSpawnController != null) {
                beatmapObjectSpawnController.obstacleDiStartMovementEvent += HandleObstacleDidStartMovementEvent;
                beatmapObjectSpawnController.noteWasCutEvent += HandleNoteWasCutEvent;
                beatmapObjectSpawnController.noteWasMissedEvent += HandleNoteWasMissedEvent;
            }

            scoreController = GameObject.FindObjectsOfType<ScoreController>().FirstOrDefault();
            if (scoreController != null) scoreController.comboDidChangeEvent += ComboChangedEvent;

            VFX.TechnicolourController.InitializeGradients();
            if (ColourManager.TechnicolourSabers) {
                Saber[] sabers = GameObject.FindObjectsOfType<Saber>();
                if (sabers != null) {
                    VFX.TechnicolourController.InitializeSabers(sabers);
                }
            }

            IsLoadingSong = false;
        }

        private BeatmapObjectCallbackController GetBeatmapObjectCallbackController() {
            BeatmapObjectCallbackController s = GameObject.FindObjectOfType<BeatmapObjectCallbackController>();
            if (s == null) {
                s = UnityEngine.Resources.FindObjectsOfTypeAll<BeatmapObjectCallbackController>().FirstOrDefault();
            }
            return s;
        }

        private void GCSSFound(Scene scene, BeatmapObjectCallbackController gcss) {
            ChromaLogger.Log("Found BOCC!", ChromaLogger.Level.DEBUG);

            _playerController = FindObjectOfType<PlayerController>();
            if (_playerController == null) ChromaLogger.Log("Player Controller not found!", ChromaLogger.Level.WARNING);

            if (gcss == null) {
                ChromaLogger.Log("Failed to obtain MainGameSceneSetup", ChromaLogger.Level.WARNING);
                return;
            }

            //GameplayCoreSetupData mgData = ReflectionUtil.GetPrivateField<MainGameSceneSetupData>(mgs, "_mainGameSceneSetupData");
            BS_Utils.Gameplay.LevelData levelData = BS_Utils.Plugin.LevelData;
            if (!levelData.IsSet) {
                ChromaLogger.Log("BS_Utils LevelData is not set", ChromaLogger.Level.WARNING);
                return;
            }
            PlayerSpecificSettings playerSettings = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.playerSpecificSettings;

            //Map

            BeatmapData _beatmapData = gcss.GetPrivateField<BeatmapData>("_beatmapData");
            if (_beatmapData == null) ChromaLogger.Log("{XXX} : NULL BEATMAP DATA", ChromaLogger.Level.ERROR);
            //BeatmapData beatmapData = CreateTransformedBeatmapData(mgData.difficultyLevel.beatmapData, mgData.gameplayOptions, mgData.gameplayMode);
            BeatmapData beatmapData = CreateTransformedBeatmapData(_beatmapData, playerSettings, BaseGameMode.CurrentBaseGameMode);
            if (beatmapData != null) {
                gcss.SetPrivateField("_beatmapData", beatmapData);
            }

            foreach (IChromaBehaviourExtension extension in extensions) extension.PostInitialization(songBPM, beatmapData, playerSettings, scoreController);

            //modes = GetModes(mgData.gameplayMode, chromaSong);

            if (ChromaConfig.DebugMode) {
                Console.WriteLine();
                Console.WriteLine();
                ChromaLogger.Log("Gamemode: " + BaseGameMode.CurrentBaseGameMode.ToString() + " -- Party: "+BaseGameMode.PartyMode, ChromaLogger.Level.DEBUG);
            }

            ColourManager.RefreshLights();

            if (ChromaConfig.LightshowModifier) {
                foreach (Saber saber in GameObject.FindObjectsOfType<Saber>()) {
                    saber.gameObject.SetActive(false);
                }
            }

            // Custom Events
            Dictionary<string, List<CustomEventData>> _customEventData = ((CustomBeatmapData)_beatmapData).customEventData;
            foreach (KeyValuePair<string, List<CustomEventData>> n in _customEventData) {
                switch (n.Key) {
                    case "_obstacleColor":
                        ChromaObstacleColourEvent.Activate(n.Value);
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
            if (ChromaBehaviour.LightingRegistered) {
                cecc.AddCustomEventCallback(ChromaGradientEvent.Activate, "_lightGradient", 0);
            }
        }

        private BeatmapData CreateTransformedBeatmapData(BeatmapData beatmapData, PlayerSpecificSettings playerSettings, BaseGameModeType baseGameMode) {
            try {
                if (beatmapData == null) throw new Exception("Null beatmapData");
                if (ChromaConfig.CustomMapCheckingEnabled) {
                    /*if (ModeActive(ChromaMode.DOUBLES_DOTS) || ModeActive(ChromaMode.DOUBLES_MONO) || ModeActive(ChromaMode.DOUBLES_REMOVED) || ModeActive(ChromaMode.INVERT_COLOUR) || ModeActive(ChromaMode.MIRROR_DIRECTION) || ModeActive(ChromaMode.MIRROR_POSITION) || ModeActive(ChromaMode.MONOCHROME) || ModeActive(ChromaMode.NO_ARROWS) || ModeActive(ChromaMode.RANDOM_COLOURS_CHROMA) || ModeActive(ChromaMode.RANDOM_COLOURS_INTENSE) || ModeActive(ChromaMode.RANDOM_COLOURS_ORIGINAL) || ModeActive(ChromaMode.RANDOM_COLOURS_TRUE)) {*/
                    ChromaLogger.Log("Attempting map modification...");
                    //return ChromaToggle.Beatmap.Z_MapModifier.CreateTransformedData(beatmapData, modes);
                    ChromaBehaviour chroma = this;
                    CustomBeatmap customBeatmap = ChromaMapModifier.CreateTransformedData(beatmapData, ref chroma, ref playerSettings, ref baseGameMode, ref songBPM);
                    if (customBeatmap == null) {
                        ChromaLogger.Log("Major error sir, beatmap data failed!", ChromaLogger.Level.WARNING);
                        return beatmapData;
                    } else {
                        return customBeatmap.BeatmapData;
                    }
                }
            } catch (Exception e) {
                ChromaLogger.Log("Error creating transformed map data...");
                ChromaLogger.Log(e, ChromaLogger.Level.ERROR);
            }
            return beatmapData;
        }

        private void HandleObstacleDidStartMovementEvent(BeatmapObjectSpawnController obstacleSpawnController, ObstacleController obstacleController) {
            StretchableObstacle stretchableObstacle = ReflectionUtil.GetPrivateField<StretchableObstacle>(obstacleController, "_stretchableObstacle");
            ChromaHandleBarrierSpawnedEvent?.Invoke(ref stretchableObstacle, ref obstacleSpawnController, ref obstacleController);
        }

        private void HandleNoteWasCutEvent(BeatmapObjectSpawnController noteSpawnController, INoteController noteController, NoteCutInfo noteCutInfo) {
            ChromaHandleNoteWasCutEvent?.Invoke(noteSpawnController, noteController, noteCutInfo);
        }

        private void HandleNoteWasMissedEvent(BeatmapObjectSpawnController noteSpawnController, INoteController noteController) {
            ChromaHandleNoteWasMissedEvent?.Invoke(noteSpawnController, noteController);
        }

        private void ComboChangedEvent(int newCombo) {
            ChromaHandleComboChangeEvent?.Invoke(newCombo);
        }

    }

}
