using HarmonyLib;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapObjectManager))]
    [HarmonyPatch("RemoveNoteControllerEventCallbacks")]
    internal class BeatmapObjectManagerRemoveNoteControllerEventCallbacks
    {
        private static void Postfix(NoteController noteController)
        {
            noteController.noteWasCutEvent -= Events.ChromaNoteColourEvent.SaberColour;
        }
    }
}