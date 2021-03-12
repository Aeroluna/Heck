namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;

    [NoodlePatch(typeof(GameNoteController))]
    [NoodlePatch("NoteDidStartJump")]
    internal static class GameNoteControllerNoteDidStartJump
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(GameNoteController __instance)
        {
            return FakeNoteHelper.GetCuttable(__instance.noteData);
        }
    }
}
