using HarmonyLib;
using IPA;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;
using UnityEngine;

namespace NoodleExtensions
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        internal const string CAPABILITY = "Noodle Extensions";
        internal const string HARMONYID_CORE = "com.noodle.BeatSaber.NoodleExtensionsCore";
        internal const string HARMONYID = "com.noodle.BeatSaber.NoodleExtensions";

        #region fun notes

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

        #endregion fun notes

        internal static readonly Vector3 _vectorZero = Vector3.zero;
        internal static readonly Quaternion _quaternionIdentity = Quaternion.identity;

        [Init]
        public void Init(IPALogger pluginLogger)
        {
            Logger.logger = pluginLogger;
            NoodleController.InitNoodlePatches();
        }

        internal static readonly Harmony coreharmony = new Harmony(HARMONYID_CORE);
        internal static readonly Harmony harmony = new Harmony(HARMONYID);

        [OnEnable]
        public void OnEnable()
        {
            SongCore.Collections.RegisterCapability(CAPABILITY);
            coreharmony.PatchAll(Assembly.GetExecutingAssembly());
            HarmonyPatches.BeatmapDataLoaderProcessNotesInTimeRow.PatchBeatmapDataLoader(coreharmony);

            CustomJSONData.CustomEventCallbackController.customEventCallbackControllerInit += Animation.AnimationController.CustomEventCallbackInit;
        }

        [OnDisable]
        public void OnDisable()
        {
            SongCore.Collections.DeregisterizeCapability(CAPABILITY);
            coreharmony.UnpatchAll(HARMONYID_CORE);
            coreharmony.UnpatchAll(HARMONYID);
        }
    }
}