using HarmonyLib;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapObjectManager))]
    [HarmonyPatch("RemoveObstacleEventCallbacks")]
    internal class BeatmapObjectManagerRemoveObstacleEventCallbacks
    {
        private static void Postfix(ObstacleController obstacleController)
        {
            if (VFX.TechnicolourController.Instantiated())
                VFX.TechnicolourController.Instance._obstacleControllers.Remove(obstacleController);
        }
    }

    [HarmonyPatch(typeof(BeatmapObjectManager))]
    [HarmonyPatch("RemoveNoteControllerEventCallbacks")]
    internal class BeatmapObjectManagerRemoveNoteControllerEventCallbacks
    {
        private static void Postfix(NoteController noteController)
        {
            if (VFX.TechnicolourController.Instantiated())
                VFX.TechnicolourController.Instance._bombControllers.Remove(noteController);
            noteController.noteWasCutEvent -= Events.ChromaNoteColourEvent.SaberColour;
        }
    }
}