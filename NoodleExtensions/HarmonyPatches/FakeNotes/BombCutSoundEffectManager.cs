using Heck;
using JetBrains.Annotations;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HeckPatch(typeof(BombCutSoundEffectManager))]
    [HeckPatch("HandleNoteWasCut")]
    internal static class BombCutSoundEffectManagerHandleNoteWasCut
    {
        // Do not create a BombCutSoundEffect for fake notes
        [UsedImplicitly]
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
