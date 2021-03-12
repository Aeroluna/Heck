namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;

    [NoodlePatch(typeof(BombNoteController))]
    [NoodlePatch("Init")]
    internal static class BombNoteControllerInit
    {
        [HarmonyPriority(Priority.High)]
        private static void Postfix(NoteData noteData, CuttableBySaber ____cuttableBySaber)
        {
            if (!FakeNoteHelper.GetCuttable(noteData))
            {
                ____cuttableBySaber.canBeCut = false;
            }
        }
    }
}
