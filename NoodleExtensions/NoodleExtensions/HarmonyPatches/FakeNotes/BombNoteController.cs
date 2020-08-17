namespace NoodleExtensions.HarmonyPatches
{
    using HarmonyLib;

    [NoodlePatch(typeof(BombNoteController))]
    [NoodlePatch("Init")]
    internal static class BombNoteControllerInit
    {
        [HarmonyPriority(Priority.High)]
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(NoteData noteData, CuttableBySaber ____cuttableBySaber)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            if (!FakeNoteHelper.GetCuttable(noteData))
            {
                ____cuttableBySaber.canBeCut = false;
            }
        }
    }
}
