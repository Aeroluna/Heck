namespace Chroma.HarmonyPatches
{
    using Chroma.Events;
    using HarmonyLib;

    [HarmonyPatch(typeof(BeatmapObjectManager))]
    [HarmonyPatch("RemoveNoteControllerEventCallbacks")]
    internal class BeatmapObjectManagerRemoveNoteControllerEventCallbacks
    {
        private static void Postfix(NoteController noteController)
        {
            noteController.noteWasCutEvent -= ChromaNoteColorEvent.SaberColor;
            ChromaNoteColorEvent.SavedNoteColors.Remove(noteController);
        }
    }
}
