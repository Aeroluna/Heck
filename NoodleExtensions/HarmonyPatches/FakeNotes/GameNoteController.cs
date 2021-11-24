using HarmonyLib;
using Heck;
using JetBrains.Annotations;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HeckPatch(typeof(GameNoteController))]
    [HeckPatch("NoteDidStartJump")]
    internal static class GameNoteControllerNoteDidStartJump
    {
        [UsedImplicitly]
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(GameNoteController __instance)
        {
            return FakeNoteHelper.GetCuttable(__instance.noteData);
        }
    }
}
