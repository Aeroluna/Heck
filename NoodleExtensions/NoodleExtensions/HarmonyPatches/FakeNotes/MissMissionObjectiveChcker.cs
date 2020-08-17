namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;

    [NoodlePatch(typeof(MissMissionObjectiveChecker))]
    [NoodlePatch("HandleNoteWasMissed")]
    internal static class MissMissionObjectiveCheckerHandleNoteWasMissed
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(INoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
