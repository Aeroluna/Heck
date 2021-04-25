namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;
    using UnityEngine;
    using static ChromaObjectDataManager;

    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("HandleNoteControllerDidInit")]
    internal static class ColorNoteVisualsHandleNoteControllerDidInitColorizerInit
    {
        [HarmonyPriority(Priority.High)]
        private static void Prefix(ColorNoteVisuals __instance, NoteController noteController)
        {
            if (!(noteController is MultiplayerConnectedPlayerNoteController) && !(noteController is TutorialNoteController))
            {
                NoteColorizer.CNVStart(__instance, noteController);
            }
        }
    }

    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("HandleNoteControllerDidInit")]
    internal static class ColorNoteVisualsHandleNoteControllerDidInitColorizer
    {
        [HarmonyPriority(Priority.Low)]
        private static void Prefix(NoteController noteController)
        {
            if (!(noteController is MultiplayerConnectedPlayerNoteController) && !(noteController is TutorialNoteController))
            {
                NoteColorizer.EnableNoteColorOverride(noteController);
            }
        }

        private static void Postfix()
        {
            NoteColorizer.DisableNoteColorOverride();
        }
    }

    [ChromaPatch(typeof(ColorNoteVisuals))]
    [ChromaPatch("HandleNoteControllerDidInit")]
    internal static class ColorNoteVisualsHandleNoteControllerDidInit
    {
        private static void Prefix(NoteController noteController)
        {
            if (!(noteController is MultiplayerConnectedPlayerNoteController))
            {
                ChromaNoteData chromaData = TryGetObjectData<ChromaNoteData>(noteController.noteData);
                if (chromaData == null)
                {
                    return;
                }

                Color? color = chromaData.Color;

                if (color.HasValue)
                {
                    noteController.SetNoteColors(color.Value, color.Value);
                }
                else
                {
                    noteController.Reset();
                }
            }
        }
    }
}
