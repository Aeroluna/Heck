namespace NoodleExtensions.HarmonyPatches
{
    using Heck;

    [HeckPatch(typeof(BadNoteCutEffectSpawner))]
    [HeckPatch("HandleNoteWasCut")]
    internal static class BadNoteCutEffectSpawnerHandleNoteWasCut
    {
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
