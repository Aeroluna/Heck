namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;

    [HarmonyPatch(typeof(NoteCutScoreSpawner))]
    [HarmonyPatch("HandleNoteWasCut")]
    internal static class NoteCutScoreSpawnerHandleNoteWasCut
    {
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
