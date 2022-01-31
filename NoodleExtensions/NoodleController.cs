using Heck;

namespace NoodleExtensions
{
    internal enum PatchType
    {
        Features
    }

    public static class NoodleController
    {
        public const string CAPABILITY = "Noodle Extensions";

        public const string ID = "NoodleExtensions";

        public const string HARMONY_ID = "com.aeroluna.NoodleExtensions";

        public const string ANIMATION = "_animation";
        public const string CUT_DIRECTION = "_cutDirection";
        public const string CUTTABLE = "_interactable";
        public const string DEFINITE_POSITION = "_definitePosition";
        public const string DISSOLVE = "_dissolve";
        public const string DISSOLVE_ARROW = "_dissolveArrow";
        public const string FAKE_NOTE = "_fake";
        public const string FLIP = "_flip";
        public const string LOCAL_ROTATION = "_localRotation";
        public const string NOTE_GRAVITY_DISABLE = "_disableNoteGravity";
        public const string NOTE_JUMP_SPEED = "_noteJumpMovementSpeed";
        public const string NOTE_LOOK_DISABLE = "_disableNoteLook";
        public const string NOTE_SPAWN_OFFSET = "_noteJumpStartBeatOffset";
        public const string POSITION = "_position";
        public const string ROTATION = "_rotation";
        public const string SCALE = "_scale";
        public const string TIME = "_time";
        public const string TRACK = "_track";
        public const string WORLD_POSITION_STAYS = "_worldPositionStays";

        public const string PARENT_TRACK = "_parentTrack";
        public const string CHILDREN_TRACKS = "_childrenTracks";

        public const string ASSIGN_PLAYER_TO_TRACK = "AssignPlayerToTrack";
        public const string ASSIGN_TRACK_PARENT = "AssignTrackParent";

        internal static HeckPatcher CorePatcher { get; } = new(HARMONY_ID + "Core");

        internal static HeckPatcher FeaturesPatcher { get; } = new(HARMONY_ID + "Features", PatchType.Features);

        internal static CustomDataDeserializer Deserializer { get; } = DeserializerManager.RegisterDeserialize<CustomDataManager>(ID);

        internal static Module FeaturesModule { get; } = ModuleManager.RegisterModule<ModuleCallbacks>(
            "Noodle",
            1,
            RequirementType.Condition,
            null,
            new[] { "Heck" });
    }
}
