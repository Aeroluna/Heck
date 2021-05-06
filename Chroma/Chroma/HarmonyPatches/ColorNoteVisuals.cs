namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;
    using Heck;
    using UnityEngine;
    using static ChromaObjectDataManager;

    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("HandleNoteControllerDidInit")]
    internal static class ColorNoteVisualsHandleNoteControllerDidInitColorizerInit
    {
        [HarmonyPriority(Priority.High)]
        private static void Prefix(ColorNoteVisuals __instance, NoteController noteController)
        {
            NoteColorizer.CNVStart(__instance, noteController);
        }
    }

    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("HandleNoteControllerDidInit")]
    internal static class ColorNoteVisualsHandleNoteControllerDidInitColorizer
    {
        [HarmonyPriority(Priority.Low)]
        private static void Prefix(NoteController noteController)
        {
            NoteColorizer.EnableNoteColorOverride(noteController);
        }

        private static void Postfix()
        {
            NoteColorizer.DisableNoteColorOverride();
        }
    }

    [HeckPatch(typeof(ColorNoteVisuals))]
    [HeckPatch("HandleNoteControllerDidInit")]
    internal static class ColorNoteVisualsHandleNoteControllerDidInit
    {
        private static void Prefix(NoteController noteController)
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
