using System;
using System.Collections;
using Chroma.Colorizer;
using Chroma.HarmonyPatches.EnvironmentComponent;
using Chroma.Lighting;
using Chroma.Lighting.EnvironmentEnhancement;
using Chroma.Settings;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chroma
{
    public static class ChromaController
    {
        internal const string CAPABILITY = "Chroma";
        internal const string HARMONY_ID_CORE = "com.noodle.BeatSaber.ChromaCore";
        internal const string HARMONY_ID = "com.noodle.BeatSaber.Chroma";

        internal const string ANIMATION = "_animation";
        internal const string COLOR = "_color";
        internal const string COUNTER_SPIN = "_counterSpin";
        internal const string DIRECTION = "_direction";
        internal const string DISABLE_SPAWN_EFFECT = "_disableSpawnEffect";
        internal const string DURATION = "_duration";
        internal const string EASING = "_easing";
        internal const string END_COLOR = "_endColor";
        internal const string ENVIRONMENT_REMOVAL = "_environmentRemoval";
        internal const string LERP_TYPE = "_lerpType";
        internal const string LIGHT_GRADIENT = "_lightGradient";
        internal const string LIGHT_ID = "_lightID";
        internal const string LOCK_POSITION = "_lockPosition";
        internal const string NAME_FILTER = "_nameFilter";
        internal const string PRECISE_SPEED = "_preciseSpeed";
        internal const string PROP = "_prop";
        internal const string PROPAGATION_ID = "_propID";
        internal const string PROP_MULT = "_propMult";
        internal const string RESET = "_reset";
        internal const string SPEED = "_speed";
        internal const string SPEED_MULT = "_speedMult";
        internal const string START_COLOR = "_startColor";
        internal const string STEP = "_step";
        internal const string STEP_MULT = "_stepMult";
        internal const string ROTATION = "_rotation";

        internal const string ENVIRONMENT = "_environment";
        internal const string ID = "_id";
        internal const string LOOKUP_METHOD = "_lookupMethod";
        internal const string DUPLICATION_AMOUNT = "_duplicate";
        internal const string ACTIVE = "_active";
        internal const string SCALE = "_scale";
        internal const string POSITION = "_position";
        internal const string LOCAL_POSITION = "_localPosition";
        internal const string OBJECT_ROTATION = "_rotation";
        internal const string LOCAL_ROTATION = "_localRotation";

        internal const string ASSIGN_FOG_TRACK = "AssignFogTrack";
        internal const string ATTENUATION = "_attenuation";
        internal const string OFFSET = "_offset";
        internal const string HEIGHT_FOG_STARTY = "_startY";
        internal const string HEIGHT_FOG_HEIGHT = "_height";

        private static readonly FieldAccessor<BeatmapObjectSpawnController, IBeatmapObjectCallbackController>.Accessor _callbackControllerAccessor
            = FieldAccessor<BeatmapObjectSpawnController, IBeatmapObjectCallbackController>.GetAccessor("_beatmapObjectCallbackController");

        private static readonly FieldAccessor<BeatmapObjectSpawnController, IBeatmapObjectSpawner>.Accessor _beatmapObjectSpawnAccessor
            = FieldAccessor<BeatmapObjectSpawnController, IBeatmapObjectSpawner>.GetAccessor("_beatmapObjectSpawner");

        private static readonly FieldAccessor<BeatmapObjectCallbackController, IAudioTimeSource>.Accessor _audioTimeSourceAccessor
            = FieldAccessor<BeatmapObjectCallbackController, IAudioTimeSource>.GetAccessor("_audioTimeSource");

        private static readonly FieldAccessor<BeatmapObjectCallbackController, IReadonlyBeatmapData>.Accessor _beatmapDataAccessor
            = FieldAccessor<BeatmapObjectCallbackController, IReadonlyBeatmapData>.GetAccessor("_beatmapData");

        public static bool ChromaIsActive { get; private set; }

        public static bool DoColorizerSabers { get; set; }

        internal static Harmony HarmonyInstanceCore { get; } = new(HARMONY_ID_CORE);

        internal static Harmony HarmonyInstance { get; } = new(HARMONY_ID);

        internal static bool SiraUtilInstalled { get; set; }

        internal static BeatmapObjectSpawnController? BeatmapObjectSpawnController { get; private set; }

        internal static IAudioTimeSource? IAudioTimeSource { get; private set; }

        public static void ToggleChromaPatches(bool value)
        {
            HeckPatchDataManager.TogglePatches(HarmonyInstance, value);
            ChromaIsActive = value;
        }

        internal static IEnumerator DelayedStart(BeatmapObjectSpawnController beatmapObjectSpawnController)
        {
            yield return new WaitForEndOfFrame();
            BeatmapObjectSpawnController = beatmapObjectSpawnController;

            // prone to breaking if anything else implements these interfaces
            BeatmapObjectManager beatmapObjectManager = (BeatmapObjectManager)_beatmapObjectSpawnAccessor(ref beatmapObjectSpawnController);
            BeatmapObjectCallbackController coreSetup = (BeatmapObjectCallbackController)_callbackControllerAccessor(ref beatmapObjectSpawnController);

            IAudioTimeSource = _audioTimeSourceAccessor(ref coreSetup);
            IReadonlyBeatmapData beatmapData = _beatmapDataAccessor(ref coreSetup);

            beatmapObjectManager.noteWasCutEvent -= NoteColorizer.ColorizeSaber;
            beatmapObjectManager.noteWasCutEvent += NoteColorizer.ColorizeSaber;

            if (!ChromaIsActive)
            {
                yield break;
            }

            if (!ChromaConfig.Instance.EnvironmentEnhancementsDisabled && beatmapData is CustomBeatmapData customBeatmap)
            {
                EnvironmentEnhancementManager.Init(customBeatmap, beatmapObjectSpawnController.noteLinesDistance);
            }

            try
            {
                // please let me kill legacy
                LegacyLightHelper.Activate(beatmapData.beatmapEventsData);
            }
            catch (Exception e)
            {
                Log.Logger.Log("Could not run Legacy Chroma Lights");
                Log.Logger.Log(e);
            }
        }

        internal static void OnActiveSceneChanged(Scene current, Scene _)
        {
            if (current.name != "GameCore")
            {
                return;
            }

            BombColorizer.Reset();
            LightColorizer.Reset();
            NoteColorizer.Reset();
            ObstacleColorizer.Reset();
            SaberColorizer.Reset();

            TrackLaneRingsManagerAwake.RingManagers.Clear();
        }
    }
}
