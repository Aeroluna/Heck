namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;

    [NoodlePatch(typeof(MissedNoteEffectSpawner))]
    [NoodlePatch("HandleNoteWasMissed")]
    internal static class MissedNoteEffectSpawnerHandleNoteWasMissed
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(INoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
