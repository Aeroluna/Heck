namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(NoteCutScoreSpawner))]
    [NoodlePatch("HandleNoteWasCut")]
    internal static class NoteCutScoreSpawnerHandleNoteWasCut
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
