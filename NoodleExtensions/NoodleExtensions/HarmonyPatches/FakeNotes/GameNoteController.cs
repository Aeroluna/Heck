namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;

    [NoodlePatch(typeof(GameNoteController))]
    [NoodlePatch("NoteDidStartJump")]
    internal static class GameNoteControllerNoteDidStartJump
    {
        [HarmonyPriority(Priority.High)]
#pragma warning disable SA1313
        private static bool Prefix(GameNoteController noteController)
#pragma warning restore SA1313
        {
            return FakeNoteHelper.GetCuttable(noteController.noteData);
        }
    }
}
