namespace NoodleExtensions.HarmonyPatches
{
    using Heck;

    [HeckPatch(typeof(BombCutSoundEffectManager))]
    [HeckPatch("HandleNoteWasCut")]
    internal static class BombCutSoundEffectManagerHandleNoteWasCut
    {
        // Do not create a BombCutSoundEffect for fake notes
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
