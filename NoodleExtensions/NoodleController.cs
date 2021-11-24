using CustomJSONData;
using HarmonyLib;
using Heck;
using NoodleExtensions.Animation;

namespace NoodleExtensions
{
    public static class NoodleController
    {
        internal const string CAPABILITY = "Noodle Extensions";
        internal const string HARMONY_ID_CORE = "com.aeroluna.BeatSaber.NoodleExtensionsCore";
        internal const string HARMONY_ID = "com.aeroluna.BeatSaber.NoodleExtensions";

        internal const string ANIMATION = "_animation";
        internal const string CUT_DIRECTION = "_cutDirection";
        internal const string CUTTABLE = "_interactable";
        internal const string DEFINITE_POSITION = "_definitePosition";
        internal const string DISSOLVE = "_dissolve";
        internal const string DISSOLVE_ARROW = "_dissolveArrow";
        internal const string FAKE_NOTE = "_fake";
        internal const string FLIP = "_flip";
        internal const string LOCAL_ROTATION = "_localRotation";
        internal const string NOTE_GRAVITY_DISABLE = "_disableNoteGravity";
        internal const string NOTE_JUMP_SPEED = "_noteJumpMovementSpeed";
        internal const string NOTE_LOOK_DISABLE = "_disableNoteLook";
        internal const string NOTE_SPAWN_OFFSET = "_noteJumpStartBeatOffset";
        internal const string POSITION = "_position";
        internal const string ROTATION = "_rotation";
        internal const string SCALE = "_scale";
        internal const string TIME = "_time";
        internal const string TRACK = "_track";
        internal const string WORLD_POSITION_STAYS = "_worldPositionStays";

        internal const string PARENT_TRACK = "_parentTrack";
        internal const string CHILDREN_TRACKS = "_childrenTracks";

        internal const string ASSIGN_PLAYER_TO_TRACK = "AssignPlayerToTrack";
        internal const string ASSIGN_TRACK_PARENT = "AssignTrackParent";

        public static bool NoodleExtensionsActive { get; private set; }

        internal static Harmony HarmonyInstanceCore { get; } = new(HARMONY_ID_CORE);

        internal static Harmony HarmonyInstance { get; } = new(HARMONY_ID);

        public static void ToggleNoodlePatches(bool value)
        {
            if (value == NoodleExtensionsActive)
            {
                return;
            }

            HeckPatchDataManager.TogglePatches(HarmonyInstance, value);

            NoodleExtensionsActive = value;
            if (NoodleExtensionsActive)
            {
                CustomEventCallbackController.didInitEvent += AnimationController.CustomEventCallbackInit;
            }
            else
            {
                CustomEventCallbackController.didInitEvent -= AnimationController.CustomEventCallbackInit;
            }
        }
    }
}
