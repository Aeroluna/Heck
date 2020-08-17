namespace Chroma.HarmonyPatches
{
    using Chroma.Utils;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;

    [ChromaPatch(typeof(ColorNoteVisuals))]
    [ChromaPatch("HandleNoteControllerDidInitEvent")]
    internal static class ColorNoteVisualsHandleNoteControllerDidInitEvent
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Prefix(ColorNoteVisuals __instance, NoteController noteController)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            if (noteController.noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                dynData.colorNoteVisuals = __instance;

                Color? color = ChromaUtils.GetColorFromData(dynData);

                if (color.HasValue)
                {
                    dynData.color = color.Value;
                }
            }

            NoteColorManager.EnableNoteColorOverride(noteController);
        }

        private static void Postfix()
        {
            NoteColorManager.DisableNoteColorOverride();
        }
    }
}
