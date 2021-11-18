namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;
    using Heck;

    [HeckPatch(typeof(GameNoteController))]
    [HeckPatch("NoteDidStartJump")]
    internal static class GameNoteControllerNoteDidStartJump
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(GameNoteController __instance)
        {
            return FakeNoteHelper.GetCuttable(__instance.noteData);
        }
    }
}
