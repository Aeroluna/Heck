namespace Chroma.HarmonyPatches
{
    using HarmonyLib;

    [HarmonyPatch(typeof(BeatmapObjectManager))]
    [HarmonyPatch("RemoveNoteControllerEventCallbacks")]
    internal class BeatmapObjectManagerRemoveNoteControllerEventCallbacks
    {
        private static void Postfix(NoteController noteController)
        {
            noteController.noteWasCutEvent -= Events.ChromaNoteColorEvent.SaberColor;
        }
    }
}
