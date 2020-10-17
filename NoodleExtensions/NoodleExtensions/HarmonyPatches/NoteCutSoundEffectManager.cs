namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(NoteCutSoundEffectManager))]
    [NoodlePatch("HandleNoteWasSpawned")]
    internal static class NoteCutSoundEffectManagerHandleNoteWasSpawned
    {
        // Do not create a NoteCutSoundEffect for fake notes
        private static bool Prefix(NoteController noteController, MonoMemoryPoolContainer<NoteCutSoundEffect> ____noteCutSoundEffectPoolContainer)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
