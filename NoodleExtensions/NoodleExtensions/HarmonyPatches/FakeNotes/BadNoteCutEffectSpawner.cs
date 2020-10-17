namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;

    [HarmonyPatch(typeof(BadNoteCutEffectSpawner))]
    [HarmonyPatch("HandleNoteWasCut")]
    internal static class BadNoteCutEffectSpawnerHandleNoteWasCut
    {
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
