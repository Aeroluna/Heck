namespace Heck
{
    internal enum PatchType
    {
        Features
    }

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

        internal const string ANIMATE_TRACK = "AnimateTrack";
        internal const string ASSIGN_PATH_ANIMATION = "AssignPathAnimation";
        internal const string INVOKE_EVENT = "InvokeEvent";

        internal const string ID = "Heck";
        internal const string HARMONY_ID = "com.aeroluna.Heck";

        public static bool DebugMode { get; internal set; }

        internal static HeckPatcher CorePatcher { get; } = new(HARMONY_ID + "Core");

        internal static HeckPatcher FeaturesPatcher { get; } = new(HARMONY_ID + "Features", PatchType.Features);

        internal static CustomDataDeserializer Deserializer { get; } = DeserializerManager.RegisterDeserialize<CustomDataManager>(ID);
    }
}
