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

namespace Chroma {

    public class ChromaBehaviour : MonoBehaviour {

        private static bool isLoadingSong = false;
        public static bool IsLoadingSong {
            get { return isLoadingSong; }
            set {
                if (value) LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ResetRandom();
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

            if (SceneUtils.IsTargetGameScene(SceneManager.GetActiveScene().buildIndex)) {
                GameObject instanceObject = new GameObject("ChromaBehaviour");
                ChromaBehaviour behaviour = instanceObject.AddComponent<ChromaBehaviour>();
                _instance = behaviour;
                ChromaLogger.Log("ChromaBehaviour instantiated.", ChromaLogger.Level.DEBUG);
                return behaviour;
            } else {
                ChromaLogger.Log("Invalid scene index.");
                return null;
            }
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
        public delegate void ChromaHandleNoteWasCut(BeatmapObjectSpawnController noteSpawnController, NoteController noteController, NoteCutInfo noteCutInfo);

        public event ChromaHandleNoteWasMissed ChromaHandleNoteWasMissedEvent;
        public delegate void ChromaHandleNoteWasMissed(BeatmapObjectSpawnController noteSpawnController, NoteController noteController);

        public event ChromaHandleBarrierSpawned ChromaHandleBarrierSpawnedEvent;
        public delegate void ChromaHandleBarrierSpawned(ref StretchableObstacle stretchableObstacle, ref StretchableCube stretchableCoreOutside, ref StretchableCube stretchableCoreInside, ref BeatmapObjectSpawnController obstacleSpawnController, ref ObstacleController obstacleController, ref bool didRecolour);

        public event ChromaHandleComboChange ChromaHandleComboChangeEvent;
        public delegate void ChromaHandleComboChange(int newCombo);

        //public event ChromaHandleNoteScaling ChromaHandleNoteScalingEvent;
        //public delegate void ChromaHandleNoteScaling(int noteID, NoteType noteType, ref float tScale);

        BeatmapObjectSpawnController beatmapObjectSpawnController;
        ScoreController scoreController;

        float songBPM = 120f;

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
        }

        void Start() {
            StartCoroutine(DelayedStart());
        }

        private IEnumerator DelayedStart() {
            yield return new WaitForSeconds(0f);
            ChromaBehaviourInstantiated?.Invoke(this);
            beatmapObjectSpawnController = UnityEngine.Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First();
            if (beatmapObjectSpawnController != null) {
                songBPM = beatmapObjectSpawnController.GetField<float>("_beatsPerMinute");
                ChromaLogger.Log("BPM Found : " + songBPM);
            }
            GameplayCoreSceneSetup coreSetup = GetGameplayCoreSetup();
            if (coreSetup != null) {
                ChromaLogger.Log("Found GCSS properly!", ChromaLogger.Level.DEBUG);
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
            
            VFX.VFXRainbowBarriers.Instantiate(songBPM);
            if (ColourManager.TechnicolourSabers) {
                Saber[] sabers = GameObject.FindObjectsOfType<Saber>();
                if (sabers != null) {
                    VFX.VFXRainbowSabers.Instantiate(sabers, songBPM, true, ChromaConfig.MatchTechnicolourSabers, ChromaConfig.MatchTechnicolourSabers ? 1f : 0.8f) ;
                }
            }

            //yield return new WaitForSeconds(5f);
            IsLoadingSong = false;
        }

        private GameplayCoreSceneSetup GetGameplayCoreSetup() {
            GameplayCoreSceneSetup s = GameObject.FindObjectOfType<GameplayCoreSceneSetup>();
            if (s == null) {
                s = UnityEngine.Resources.FindObjectsOfTypeAll<GameplayCoreSceneSetup>().FirstOrDefault();
            }
            return s;
        }

        private void GCSSFound(Scene scene, GameplayCoreSceneSetup gcss) {
            ChromaLogger.Log("Found GCSS!", ChromaLogger.Level.DEBUG);
            //Plugin.PlayReloadSound();

            _playerController = FindObjectOfType<PlayerController>();
            if (_playerController == null) ChromaLogger.Log("Player Controller not found!", ChromaLogger.Level.WARNING);

            if (!SceneUtils.IsTargetGameScene(scene.buildIndex)) {
                ChromaLogger.Log("Somehow we got to the point where we override a map, while not playing a map.  How did this happen?", ChromaLogger.Level.WARNING);
                return;
            }

            if (gcss == null) {
                ChromaLogger.Log("Failed to obtain MainGameSceneSetup", ChromaLogger.Level.WARNING);
                return;
            }

            //GameplayCoreSetupData mgData = ReflectionUtil.GetPrivateField<MainGameSceneSetupData>(mgs, "_mainGameSceneSetupData");
            StandardLevelSceneSetup slsSetup = Resources.FindObjectsOfTypeAll<StandardLevelSceneSetup>().FirstOrDefault();
            if (slsSetup == null) {
                ChromaLogger.Log("Failed to find StandardLevelSceneSetup", ChromaLogger.Level.WARNING);
                return;
            }
            StandardLevelSceneSetupDataSO slsData = slsSetup.standardLevelSceneSetupData;
            if (slsData == null) {
                ChromaLogger.Log("Failed to obtain StandardLevelSceneSetupDataSO from StandardLevelSceneSetup", ChromaLogger.Level.WARNING);
                return;
            }

            PlayerSpecificSettings playerSettings = slsData.gameplayCoreSetupData.playerSpecificSettings;

            ChromaLogger.Log("SLSSetup, SLSData, and PlayerSettings!", ChromaLogger.Level.DEBUG);

            //Map

            BeatmapDataModel _beatmapDataModel = ReflectionUtil.GetField<BeatmapDataModel>(gcss, "_beatmapDataModel");
            if (_beatmapDataModel == null) ChromaLogger.Log("{XXX} : NULL BEATMAP DATA MODEL", ChromaLogger.Level.ERROR);
            if (_beatmapDataModel.beatmapData == null) ChromaLogger.Log("{XXX} : NULL BEATMAP DATA MODEL BEATMAP DATA", ChromaLogger.Level.ERROR);
            //BeatmapData beatmapData = CreateTransformedBeatmapData(mgData.difficultyLevel.beatmapData, mgData.gameplayOptions, mgData.gameplayMode);
            BeatmapData beatmapData = CreateTransformedBeatmapData(_beatmapDataModel.beatmapData, playerSettings, BaseGameMode.CurrentBaseGameMode);
            if (beatmapData != null) {
                _beatmapDataModel.beatmapData = beatmapData;
                ReflectionUtil.SetField(gcss, "_beatmapDataModel", _beatmapDataModel);
            }

            foreach (IChromaBehaviourExtension extension in extensions) extension.PostInitialization(songBPM, beatmapData, playerSettings, scoreController);

            //modes = GetModes(mgData.gameplayMode, chromaSong);

            if (ChromaConfig.DebugMode) {
                Console.WriteLine();
                Console.WriteLine();
                ChromaLogger.Log("Gamemode: " + BaseGameMode.CurrentBaseGameMode.ToString() + " -- Party: "+BaseGameMode.PartyMode, ChromaLogger.Level.DEBUG);
            }

            //ChromaLogger.Log("Modify Sabers was called", ChromaLogger.Level.DEBUG);

            ColourManager.RefreshLights();

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

        #region barriers
        private void HandleObstacleDidStartMovementEvent(BeatmapObjectSpawnController obstacleSpawnController, ObstacleController obstacleController) {

            try {
                StretchableObstacle stretchableObstacle = ReflectionUtil.GetField<StretchableObstacle>(obstacleController, "_stretchableObstacle");
                StretchableCube stretchableCoreOutside = ReflectionUtil.GetField<StretchableCube>(stretchableObstacle, "_stretchableCoreOutside");
                StretchableCube stretchableCoreInside = ReflectionUtil.GetField<StretchableCube>(stretchableObstacle, "_stretchableCoreInside");
                bool didRecolour = VFX.VFXRainbowBarriers.IsRainbowWalls();

                ChromaHandleBarrierSpawnedEvent?.Invoke(ref stretchableObstacle, ref stretchableCoreOutside, ref stretchableCoreInside, ref obstacleSpawnController, ref obstacleController, ref didRecolour);

                if (!didRecolour) {
                    RecolourWall(stretchableCoreInside, obstacleController.obstacleData.time);
                    RecolourWall(stretchableCoreOutside, obstacleController.obstacleData.time);
                }
            } catch (Exception e) {
                ChromaLogger.Log(e);
            }
        }

        private void RecolourWall(StretchableCube wall, float time) {
            Color color = ColourManager.GetBarrierColour(time);

            if (color == ColourManager.DefaultBarrierColour || color == Color.clear) return;

            foreach (Transform component in wall.transform.parent.parent) {
                foreach (Transform child in component.transform) {
                    MeshRenderer ren = child.GetComponent<MeshRenderer>();
                    if (ren.material.color != Color.clear) ren.material.color = color;
                }
            }

            MeshRenderer r = wall.GetComponent<MeshRenderer>();
            float cor = (3f * ColourManager.barrierColourCorrectionScale) + 1;//4f * ColourManager.barrierColourCorrectionScale;
            r.material.SetColor("_AddColor", (color / (4f * ColourManager.barrierColourCorrectionScale)).ColorWithAlpha(0f));
        }
        #endregion

        private void HandleNoteWasCutEvent(BeatmapObjectSpawnController noteSpawnController, NoteController noteController, NoteCutInfo noteCutInfo) {
            ChromaHandleNoteWasCutEvent?.Invoke(noteSpawnController, noteController, noteCutInfo);
        }

        private void HandleNoteWasMissedEvent(BeatmapObjectSpawnController noteSpawnController, NoteController noteController) {
            ChromaHandleNoteWasMissedEvent?.Invoke(noteSpawnController, noteController);
        }

        private void ComboChangedEvent(int newCombo) {
            ChromaHandleComboChangeEvent?.Invoke(newCombo);
        }

    }

}
