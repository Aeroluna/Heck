namespace NoodleExtensions
{
    using System.Reflection;
    using HarmonyLib;
    using IPA;
    using UnityEngine;
    using IPALogger = IPA.Logging.Logger;

    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        internal const string CAPABILITY = "Noodle Extensions";
        internal const string HARMONYIDCORE = "com.noodle.BeatSaber.NoodleExtensionsCore";
        internal const string HARMONYID = "com.noodle.BeatSaber.NoodleExtensions";

        internal const string POSITION = "_position";
        internal const string ROTATION = "_rotation";

        internal const string LOCALROTATION = "_localRotation";
        internal const string NOTEJUMPSPEED = "_noteJumpMovementSpeed";
        internal const string SPAWNOFFSET = "_noteJumpStartBeatOffset";

        internal const string DURATION = "_duration";
        internal const string START = "_start";
        internal const string END = "_end";
        internal const string EASING = "_easing";

        internal const string DEFINITEPOSITION = "_definitePosition";
        internal const string DISSOLVE = "_dissolve";
        internal const string DISSOLVEARROW = "_dissolveArrow";

        internal const string POINTDEFINITIONS = "_pointDefinitions";
        internal const string NAME = "_name";
        internal const string POINTS = "_points";

        internal const string SCALE = "_scale";
        internal const string CUTDIRECTION = "_cutDirection";
        internal const string FLIP = "_flip";

        internal const string TRACK = "_track";

        internal static readonly Vector3 VectorZero = Vector3.zero;
        internal static readonly Quaternion QuaternionIdentity = Quaternion.identity;

        internal static readonly Harmony HarmonyInstanceCore = new Harmony(HARMONYIDCORE);
        internal static readonly Harmony HarmonyInstance = new Harmony(HARMONYID);

        [Init]
        public void Init(IPALogger pluginLogger)
        {
            NoodleLogger.IPAlogger = pluginLogger;
            NoodleController.InitNoodlePatches();

            Animation.TrackManager.TrackWasCreated += Animation.AnimationHelper.AddTrackProperties;
        }

        [OnEnable]
        public void OnEnable()
        {
            SongCore.Collections.RegisterCapability(CAPABILITY);
            HarmonyInstanceCore.PatchAll(Assembly.GetExecutingAssembly());
            HarmonyPatches.BeatmapDataLoaderProcessNotesInTimeRow.PatchBeatmapDataLoader(HarmonyInstanceCore);

            CustomJSONData.CustomEventCallbackController.customEventCallbackControllerInit += Animation.AnimationController.CustomEventCallbackInit;
        }

        [OnDisable]
        public void OnDisable()
        {
            SongCore.Collections.DeregisterizeCapability(CAPABILITY);
            HarmonyInstanceCore.UnpatchAll(HARMONYIDCORE);
            HarmonyInstanceCore.UnpatchAll(HARMONYID);

            CustomJSONData.CustomEventCallbackController.customEventCallbackControllerInit -= Animation.AnimationController.CustomEventCallbackInit;
        }
    }
}
