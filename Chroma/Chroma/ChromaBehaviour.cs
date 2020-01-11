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
                if (value)
                {
                    ParticleSystemEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ResetRandom();
                    LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ResetRandom();
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
        public delegate void ChromaHandleBarrierSpawned(ref StretchableObstacle stretchableObstacle, ref BeatmapObjectSpawnController obstacleSpawnController, ref ObstacleController obstacleController, ref bool didRecolour);

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
            BeatmapObjectCallbackController coreSetup = GetBeatmapObjectCallbackController();
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

        private BeatmapObjectCallbackController GetBeatmapObjectCallbackController() {
            BeatmapObjectCallbackController s = GameObject.FindObjectOfType<BeatmapObjectCallbackController>();
            if (s == null) {
                s = UnityEngine.Resources.FindObjectsOfTypeAll<BeatmapObjectCallbackController>().FirstOrDefault();
            }
            return s;
        }

        private void GCSSFound(Scene scene, BeatmapObjectCallbackController gcss) {
            ChromaLogger.Log("Found GCSS!", ChromaLogger.Level.DEBUG);
            //Plugin.PlayReloadSound();

            _playerController = FindObjectOfType<PlayerController>();
            if (_playerController == null) ChromaLogger.Log("Player Controller not found!", ChromaLogger.Level.WARNING);

            /*if (!SceneUtils.IsTargetGameScene(scene.buildIndex)) {
                ChromaLogger.Log("Somehow we got to the point where we override a map, while not playing a map.  How did this happen?", ChromaLogger.Level.WARNING);
                return;
            }*/

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

            BeatmapData _beatmapDataModel = ReflectionUtil.GetField<BeatmapData>(gcss, "_beatmapData");
            if (_beatmapDataModel == null) ChromaLogger.Log("{XXX} : NULL BEATMAP DATA", ChromaLogger.Level.ERROR);
            //if (_beatmapDataModel.beatmapData == null) ChromaLogger.Log("{XXX} : NULL BEATMAP DATA MODEL BEATMAP DATA", ChromaLogger.Level.ERROR);
            //BeatmapData beatmapData = CreateTransformedBeatmapData(mgData.difficultyLevel.beatmapData, mgData.gameplayOptions, mgData.gameplayMode);
            BeatmapData beatmapData = CreateTransformedBeatmapData(_beatmapDataModel, playerSettings, BaseGameMode.CurrentBaseGameMode);
            if (beatmapData != null) {
                ReflectionUtil.SetField(gcss, "_beatmapData", beatmapData);
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

            if (ChromaConfig.LightshowModifier) {
                foreach (Saber saber in GameObject.FindObjectsOfType<Saber>()) {
                    saber.gameObject.SetActive(false);
                }
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

        #region barriers
        private void HandleObstacleDidStartMovementEvent(BeatmapObjectSpawnController obstacleSpawnController, ObstacleController obstacleController) {

            try {
                StretchableObstacle stretchableObstacle = ReflectionUtil.GetField<StretchableObstacle>(obstacleController, "_stretchableObstacle");
                StretchableCube stretchableCore = ReflectionUtil.GetField<StretchableCube>(stretchableObstacle, "_stretchableCore");
                ParametricBoxFrameController frameController = ReflectionUtil.GetField<ParametricBoxFrameController>(stretchableObstacle, "_obstacleFrame");
                ParametricBoxFakeGlowController fakeGlowController = ReflectionUtil.GetField<ParametricBoxFakeGlowController>(stretchableObstacle, "_obstacleFakeGlow");
                float time = obstacleController.obstacleData.time;
                Color color = ColourManager.GetBarrierColour(time);
                frameController.color = color;
                fakeGlowController.color = color;
                bool didRecolour = VFX.VFXRainbowBarriers.IsRainbowWalls();

                ChromaHandleBarrierSpawnedEvent?.Invoke(ref stretchableObstacle, ref obstacleSpawnController, ref obstacleController, ref didRecolour);

                if (!didRecolour && color != ColourManager.DefaultBarrierColour && color != Color.clear) {
                    RecolourWall(stretchableCore, ColourManager.GetCorrectedBarrierColour(time));
                }
            } catch (Exception e) {
                ChromaLogger.Log(e);
            }
        }

        private void RecolourWall(StretchableCube wall, Color color) {
            //CustomUI.Utilities.UIUtilities.PrintHierarchy(wall.transform.parent);
            foreach (Transform component in wall.transform.parent) {
                foreach (Transform child in component.transform) {
                    try {
                        MeshRenderer ren = child.GetComponent<MeshRenderer>();
                        if (ren.material.color != Color.clear) ren.material.color = color;
                    } catch(Exception) {
                        // This doesn't have a color
                        // It could be the Collider
                    }
                }
            }

            MeshRenderer r = wall.GetComponent<MeshRenderer>();
            r.material.SetColor("_AddColor", color);
        }
        #endregion

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
