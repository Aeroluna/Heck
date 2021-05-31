﻿namespace Chroma
{
    using System.Collections;
    using Chroma.Colorizer;
    using Chroma.HarmonyPatches;
    using Chroma.Settings;
    using CustomJSONData.CustomBeatmap;
    using IPA.Utilities;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using static Chroma.Plugin;

    public static class ChromaController
    {
        private static readonly FieldAccessor<BeatmapObjectSpawnController, IBeatmapObjectCallbackController>.Accessor _callbackControllerAccessor = FieldAccessor<BeatmapObjectSpawnController, IBeatmapObjectCallbackController>.GetAccessor("_beatmapObjectCallbackController");
        private static readonly FieldAccessor<BeatmapObjectSpawnController, IBeatmapObjectSpawner>.Accessor _beatmapObjectSpawnAccessor = FieldAccessor<BeatmapObjectSpawnController, IBeatmapObjectSpawner>.GetAccessor("_beatmapObjectSpawner");
        private static readonly FieldAccessor<BeatmapObjectCallbackController, IAudioTimeSource>.Accessor _audioTimeSourceAccessor = FieldAccessor<BeatmapObjectCallbackController, IAudioTimeSource>.GetAccessor("_audioTimeSource");
        private static readonly FieldAccessor<BeatmapObjectCallbackController, IReadonlyBeatmapData>.Accessor _beatmapDataAccessor = FieldAccessor<BeatmapObjectCallbackController, IReadonlyBeatmapData>.GetAccessor("_beatmapData");

        public static bool ChromaIsActive { get; private set; }

        public static bool DoColorizerSabers { get; set; }

        internal static BeatmapObjectSpawnController BeatmapObjectSpawnController { get; private set; }

        internal static IAudioTimeSource IAudioTimeSource { get; private set; }

        public static void ToggleChromaPatches(bool value)
        {
            Heck.HeckData.TogglePatches(_harmonyInstance, value);
            ChromaIsActive = value;
        }

        internal static IEnumerator DelayedStart(BeatmapObjectSpawnController beatmapObjectSpawnController)
        {
            yield return new WaitForEndOfFrame();
            BeatmapObjectSpawnController = beatmapObjectSpawnController;

            // prone to breaking if anything else implements these interfaces
            BeatmapObjectManager beatmapObjectManager = _beatmapObjectSpawnAccessor(ref beatmapObjectSpawnController) as BeatmapObjectManager;
            BeatmapObjectCallbackController coreSetup = _callbackControllerAccessor(ref beatmapObjectSpawnController) as BeatmapObjectCallbackController;

            IAudioTimeSource = _audioTimeSourceAccessor(ref coreSetup);
            IReadonlyBeatmapData beatmapData = _beatmapDataAccessor(ref coreSetup);

            beatmapObjectManager.noteWasCutEvent -= NoteColorizer.ColorizeSaber;
            beatmapObjectManager.noteWasCutEvent += NoteColorizer.ColorizeSaber;

            if (ChromaIsActive)
            {
                if (beatmapData is CustomBeatmapData customBeatmap)
                {
                    if (ChromaConfig.Instance.EnvironmentEnhancementsEnabled)
                    {
                        EnvironmentEnhancementManager.Init(customBeatmap, beatmapObjectSpawnController.noteLinesDistance);
                    }
                }

                // please let me kill legacy
                LegacyLightHelper.Activate(beatmapData.beatmapEventsData);
            }
        }

        internal static void OnActiveSceneChanged(Scene current, Scene _)
        {
            if (current.name == "GameCore")
            {
                TrackLaneRingsManagerAwake.RingManagers.Clear();
            }
        }
    }
}
