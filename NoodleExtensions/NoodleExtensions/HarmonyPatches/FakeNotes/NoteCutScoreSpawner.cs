namespace NoodleExtensions.HarmonyPatches
{
    using Heck;

    [HeckPatch(typeof(NoteCutScoreSpawner))]
    [HeckPatch("HandleNoteWasCut")]
    internal static class NoteCutScoreSpawnerHandleNoteWasCut
    {
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
