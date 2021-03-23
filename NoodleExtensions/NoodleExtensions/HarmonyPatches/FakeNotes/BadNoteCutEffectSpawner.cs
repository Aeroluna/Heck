namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(BadNoteCutEffectSpawner))]
    [NoodlePatch("HandleNoteWasCut")]
    internal static class BadNoteCutEffectSpawnerHandleNoteWasCut
    {
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
