namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;

    [NoodlePatch(typeof(BadCutsMissionObjectiveChecker))]
    [NoodlePatch("HandleNoteWasCut")]
    internal static class BadCutsMissionObjectiveCheckerHandleNoteWasCut
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
