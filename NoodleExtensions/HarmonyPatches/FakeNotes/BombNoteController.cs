using HarmonyLib;
using Heck;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HeckPatch(typeof(BombNoteController))]
    [HeckPatch("Init")]
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
