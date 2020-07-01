namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;

    [NoodlePatch(typeof(ScoreController))]
    [NoodlePatch("HandleNoteWasCutEvent")]
    internal class ScoreControllerHandleNoteWasCutEvent
    {
        [HarmonyPriority(Priority.High)]
#pragma warning disable SA1313
        private static bool Prefix(INoteController noteController)
#pragma warning restore SA1313
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }

    [NoodlePatch(typeof(ScoreController))]
    [NoodlePatch("HandleNoteWasMissedEvent")]
    internal class ScoreControllerHandleNoteWasMissedEvent
    {
        [HarmonyPriority(Priority.High)]
#pragma warning disable SA1313
        private static bool Prefix(INoteController noteController)
#pragma warning restore SA1313
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
