namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(BombCutSoundEffectManager))]
    [NoodlePatch("HandleNoteWasCut")]
    internal static class BombCutSoundEffectManagerHandleNoteWasCut
    {
        // Do not create a BombCutSoundEffect for fake notes
        private static bool Prefix(NoteController noteController)
        {
            if (!(noteController is MultiplayerConnectedPlayerNoteController))
            {
                return FakeNoteHelper.GetFakeNote(noteController);
            }

            return true;
        }
    }
}
