namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;

    [NoodlePatch(typeof(GoodCutsMissionObjectiveChecker))]
    [NoodlePatch("HandleNoteWasCut")]
    internal static class GoodCutsMissionObjectiveCheckerHandleNoteWasCut
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(INoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
