using CustomJSONData.CustomBeatmap;
using Heck;

namespace NoodleExtensions
{
    internal enum PatchType
    {
        Features
    }

    internal static class NoodleController
    {
        internal const string V2_CUT_DIRECTION = "_cutDirection";
        internal const string V2_CUTTABLE = "_interactable";
        internal const string V2_DEFINITE_POSITION = "_definitePosition";
        internal const string V2_DISSOLVE = "_dissolve";
        internal const string V2_DISSOLVE_ARROW = "_dissolveArrow";
        internal const string V2_FAKE_NOTE = "_fake";
        internal const string V2_FLIP = "_flip";
        internal const string V2_NOTE_GRAVITY_DISABLE = "_disableNoteGravity";
        internal const string V2_NOTE_JUMP_SPEED = "_noteJumpMovementSpeed";
        internal const string V2_NOTE_LOOK_DISABLE = "_disableNoteLook";
        internal const string V2_NOTE_SPAWN_OFFSET = "_noteJumpStartBeatOffset";
        internal const string V2_TIME = "_time";
        internal const string V2_WORLD_POSITION_STAYS = "_worldPositionStays";
        internal const string V2_PARENT_TRACK = "_parentTrack";
        internal const string V2_CHILDREN_TRACKS = "_childrenTracks";
        internal const string V2_PLAYER_TRACK_OBJECT = "_playerTrackObject";

        internal const string NOTE_OFFSET = "coordinates";
        internal const string TAIL_NOTE_OFFSET = "tailCoordinates";
        internal const string OBSTACLE_SIZE = "size";
        internal const string WORLD_ROTATION = "worldRotation";
        internal const string INTERACTABLE = "interactable";
        internal const string UNINTERACTABLE = "uninteractable";
        internal const string OFFSET_POSITION = "offsetPosition";
        internal const string OFFSET_ROTATION = "offsetWorldRotation";
        internal const string DEFINITE_POSITION = "definitePosition";
        internal const string DISSOLVE = "dissolve";
        internal const string DISSOLVE_ARROW = "dissolveArrow";
        internal const string FLIP = "flip";
        internal const string NOTE_GRAVITY_DISABLE = "disableNoteGravity";
        internal const string NOTE_JUMP_SPEED = "noteJumpMovementSpeed";
        internal const string NOTE_LOOK_DISABLE = "disableNoteLook";
        internal const string NOTE_SPAWN_OFFSET = "noteJumpStartBeatOffset";
        internal const string TIME = "time";
        internal const string WORLD_POSITION_STAYS = "worldPositionStays";
        internal const string PARENT_TRACK = "parentTrack";
        internal const string CHILDREN_TRACKS = "childrenTracks";
        internal const string PLAYER_TRACK_OBJECT = "playerTrackObject";

        internal const string INTERNAL_STARTNOTELINELAYER = "NE_startNoteLineLayer";
        internal const string INTERNAL_TAILSTARTNOTELINELAYER = "NE_tailStartNoteLineLayer";
        internal const string INTERNAL_FLIPYSIDE = "NE_flipYSide";
        internal const string INTERNAL_FLIPLINEINDEX = "NE_flipLineIndex";
        internal const string INTERNAL_FAKE_NOTE = "NE_fake";

        internal const string ASSIGN_PLAYER_TO_TRACK = "AssignPlayerToTrack";
        internal const string ASSIGN_TRACK_PARENT = "AssignTrackParent";

        internal const string CAPABILITY = "Noodle Extensions";
        internal const string ID = "NoodleExtensions";
        internal const string HARMONY_ID = "aeroluna.NoodleExtensions";

        internal static HeckPatcher CorePatcher { get; } = new(HARMONY_ID + "Core");

        internal static HeckPatcher FeaturesPatcher { get; } = new(HARMONY_ID + "Features", PatchType.Features);

        internal static DataDeserializer Deserializer { get; } = DeserializerManager.Register<CustomDataManager>(ID);

        internal static CustomJSONDataDeserializer JSONDeserializer { get; } = CustomJSONDataDeserializer.Register<FakeNotesJSON>();

        internal static Module FeaturesModule { get; } = ModuleManager.Register<ModuleCallbacks>(
            "Noodle",
            2,
            RequirementType.Condition,
            null,
            new[] { "Heck" });
    }
}
