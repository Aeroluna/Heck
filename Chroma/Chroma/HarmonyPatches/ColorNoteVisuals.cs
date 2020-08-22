namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using Chroma.Utils;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("HandleNoteControllerDidInitEvent")]
    internal static class ColorNoteVisualsHandleNoteControllerDidInitEventColorizerInit
    {
        [HarmonyPriority(Priority.High)]
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Prefix(ColorNoteVisuals __instance, NoteController noteController)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            NoteColorizer.CNVStart(__instance, noteController);
        }
    }

    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("HandleNoteControllerDidInitEvent")]
    internal static class ColorNoteVisualsHandleNoteControllerDidInitEventColorizer
    {
        [HarmonyPriority(Priority.Low)]
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Prefix(NoteController noteController)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            NoteColorizer.EnableNoteColorOverride(noteController);
        }

        private static void Postfix()
        {
            NoteColorizer.DisableNoteColorOverride();
        }
    }

    [ChromaPatch(typeof(ColorNoteVisuals))]
    [ChromaPatch("HandleNoteControllerDidInitEvent")]
    internal static class ColorNoteVisualsHandleNoteControllerDidInitEvent
    {
        private static void Prefix(NoteController noteController)
        {
            if (noteController.noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;

                Color? color = ChromaUtils.GetColorFromData(dynData);

                if (color.HasValue)
                {
                    noteController.SetNoteColors(color.Value, color.Value);
                    dynData.color0 = color.Value;
                    dynData.color1 = color.Value;
                }
                else
                {
                    noteController.Reset();
                }
            }
        }
    }
}
