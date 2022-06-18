namespace Heck
{
    internal enum PatchType
    {
        Features
    }

    public static class HeckController
    {
        public const string V2_DURATION = "_duration";
        public const string V2_EASING = "_easing";
        public const string V2_NAME = "_name";
        public const string V2_POINT_DEFINITIONS = "_pointDefinitions";
        public const string V2_POINTS = "_points";
        public const string V2_TRACK = "_track";
        public const string V2_ANIMATION = "_animation";
        public const string V2_LOCAL_ROTATION = "_localRotation";
        public const string V2_POSITION = "_position";
        public const string V2_ROTATION = "_rotation";
        public const string V2_LOCAL_POSITION = "_localPosition";
        public const string V2_SCALE = "_scale";

        public const string DURATION = "duration";
        public const string EASING = "easing";
        public const string NAME = "name";
        public const string POINT_DEFINITIONS = "pointDefinitions";
        public const string POINTS = "points";
        public const string TRACK = "track";
        public const string ANIMATION = "animation";
        public const string POSITION = "position";
        public const string ROTATION = "rotation";
        public const string LOCAL_ROTATION = "localRotation";
        public const string LOCAL_POSITION = "localPosition";
        public const string SCALE = "scale";
        public const string EVENT = "event";
        public const string TYPE = "type";
        public const string EVENT_DEFINITIONS = "eventDefinitions";

        public const string LEFT_HANDED_ID = "leftHanded";

        internal const string ANIMATE_TRACK = "AnimateTrack";
        internal const string ASSIGN_PATH_ANIMATION = "AssignPathAnimation";
        internal const string INVOKE_EVENT = "InvokeEvent";

        internal const string ID = "Heck";
        internal const string HARMONY_ID = "aeroluna.Heck";

        public static bool DebugMode { get; internal set; }

        internal static HeckPatcher CorePatcher { get; } = new(HARMONY_ID + "Core");

        internal static HeckPatcher FeaturesPatcher { get; } = new(HARMONY_ID + "Features", PatchType.Features);

        internal static DataDeserializer Deserializer { get; } = DeserializerManager.Register<CustomDataManager>(ID);
    }
}
