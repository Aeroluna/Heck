namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(BadNoteCutEffectSpawner))]
    [NoodlePatch("HandleNoteWasCut")]
    internal static class BadNoteCutEffectSpawnerHandleNoteWasCut
    {
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
