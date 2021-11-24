using Heck;
using JetBrains.Annotations;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HeckPatch(typeof(NoteCutScoreSpawner))]
    [HeckPatch("HandleNoteWasCut")]
    internal static class NoteCutScoreSpawnerHandleNoteWasCut
    {
        [UsedImplicitly]
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
