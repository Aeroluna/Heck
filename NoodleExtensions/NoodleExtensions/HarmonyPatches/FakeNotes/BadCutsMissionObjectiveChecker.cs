namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;

    [NoodlePatch(typeof(BadCutsMissionObjectiveChecker))]
    [NoodlePatch("HandleNoteWasCut")]
    internal static class BadCutsMissionObjectiveCheckerHandleNoteWasCut
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(INoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
