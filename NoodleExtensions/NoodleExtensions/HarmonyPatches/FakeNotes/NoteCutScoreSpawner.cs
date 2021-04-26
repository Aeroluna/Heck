namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(NoteCutScoreSpawner))]
    [NoodlePatch("HandleNoteWasCut")]
    internal static class NoteCutScoreSpawnerHandleNoteWasCut
    {
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
