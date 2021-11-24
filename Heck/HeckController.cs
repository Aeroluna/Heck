using JetBrains.Annotations;

namespace Heck
{
    public static class HeckController
    {
        public const string DURATION = "_duration";
        public const string EASING = "_easing";
        public const string EVENT = "_event";
        public const string EVENT_DEFINITIONS = "_eventDefinitions";
        public const string NAME = "_name";
        public const string POINT_DEFINITIONS = "_pointDefinitions";
        public const string POINTS = "_points";
        public const string TRACK = "_track";

        public const string ANIMATE_TRACK = "AnimateTrack";
        public const string ASSIGN_PATH_ANIMATION = "AssignPathAnimation";
        public const string INVOKE_EVENT = "InvokeEvent";

        public const string HARMONY_ID = "com.aeroluna.BeatSaber.Heck";

        [PublicAPI]
        public static bool CumDump { get; internal set; }
    }
}
