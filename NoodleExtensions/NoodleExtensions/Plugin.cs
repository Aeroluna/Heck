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

        internal const string CUTDIRECTION = "_cutDirection";
        internal const string CUTTABLE = "_interactable";
        internal const string DEFINITEPOSITION = "_definitePosition";
        internal const string DISSOLVE = "_dissolve";
        internal const string DISSOLVEARROW = "_dissolveArrow";
        internal const string DURATION = "_duration";
        internal const string EASING = "_easing";
        internal const string FAKENOTE = "_fake";
        internal const string FLIP = "_flip";
        internal const string LOCALROTATION = "_localRotation";
        internal const string NAME = "_name";
        internal const string NOTEGRAVITYDISABLE = "_disableNoteGravity";
        internal const string NOTEJUMPSPEED = "_noteJumpMovementSpeed";
        internal const string NOTELOOKDISABLE = "_disableNoteLook";
        internal const string NOTESPAWNOFFSET = "_noteJumpStartBeatOffset";
        internal const string POINTDEFINITIONS = "_pointDefinitions";
        internal const string POINTS = "_points";
        internal const string POSITION = "_position";
        internal const string ROTATION = "_rotation";
        internal const string SCALE = "_scale";
        internal const string TIME = "_time";
        internal const string TRACK = "_track";

        internal const string ANIMATETRACK = "AnimateTrack";
        internal const string ASSIGNPATHANIMATION = "AssignPathAnimation";
        internal const string ASSIGNPLAYERTOTRACK = "AssignPlayerToTrack";
        internal const string ASSIGNTRACKPARENT = "AssignTrackParent";

        internal static readonly Vector3 _vectorZero = Vector3.zero;
        internal static readonly Quaternion _quaternionIdentity = Quaternion.identity;

        internal static readonly Harmony _harmonyInstanceCore = new Harmony(HARMONYIDCORE);
        internal static readonly Harmony _harmonyInstance = new Harmony(HARMONYID);

        [Init]
        public void Init(IPALogger pluginLogger)
        {
            NoodleLogger.IPAlogger = pluginLogger;
            NoodleController.InitNoodlePatches();

            Animation.TrackManager.TrackManagerCreated += Animation.AssignPlayerToTrack.OnTrackManagerCreated;
            Animation.TrackManager.TrackManagerCreated += Animation.AssignTrackParent.OnTrackManagerCreated;
            Animation.TrackManager.TrackCreated += Animation.AnimationHelper.OnTrackCreated;
        }

        [OnEnable]
        public void OnEnable()
        {
            SongCore.Collections.RegisterCapability(CAPABILITY);
            _harmonyInstanceCore.PatchAll(Assembly.GetExecutingAssembly());

            CustomJSONData.CustomBeatmap.CustomBeatmapData.CustomBeatmapDataWasCreated += FakeNoteRecount.OnCustomBeatmapDataCreated;
        }

        [OnDisable]
        public void OnDisable()
        {
            SongCore.Collections.DeregisterizeCapability(CAPABILITY);
            _harmonyInstanceCore.UnpatchAll(HARMONYIDCORE);
            _harmonyInstanceCore.UnpatchAll(HARMONYID);

            CustomJSONData.CustomBeatmap.CustomBeatmapData.CustomBeatmapDataWasCreated -= FakeNoteRecount.OnCustomBeatmapDataCreated;
        }
    }
}
