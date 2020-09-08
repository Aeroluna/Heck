namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;

    [NoodlePatch(typeof(BeatmapObjectManager))]
    [NoodlePatch("HandleNoteWasCut")]
    internal static class BeatmapObjectManagerHandleNoteWasCut
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }

    [NoodlePatch(typeof(BeatmapObjectManager))]
    [NoodlePatch("HandleNoteWasMissed")]
    internal static class BeatmapObjectManagerHandleNoteWasMissed
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
