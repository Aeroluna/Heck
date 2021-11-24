using Heck;
using JetBrains.Annotations;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HeckPatch(typeof(BadNoteCutEffectSpawner))]
    [HeckPatch("HandleNoteWasCut")]
    internal static class BadNoteCutEffectSpawnerHandleNoteWasCut
    {
        [UsedImplicitly]
        private static bool Prefix(NoteController noteController)
        {
            return FakeNoteHelper.GetFakeNote(noteController);
        }
    }
}
