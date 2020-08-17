namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;

    [NoodlePatch(typeof(ScoreController))]
    [NoodlePatch("HandleNoteWasCutEvent")]
    internal static class ScoreControllerHandleNoteWasCutEvent
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(INoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }

    [NoodlePatch(typeof(ScoreController))]
    [NoodlePatch("HandleNoteWasMissedEvent")]
    internal static class ScoreControllerHandleNoteWasMissedEvent
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(INoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
