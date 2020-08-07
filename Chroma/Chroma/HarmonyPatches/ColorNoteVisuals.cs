namespace Chroma.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Chroma.Utils;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("HandleNoteControllerDidInitEvent")]
    internal class ColorNoteVisualsHandleNoteControllerDidInitEvent
    {
#pragma warning disable SA1313
        private static void Prefix(ColorNoteVisuals __instance, NoteController noteController)
#pragma warning restore SA1313
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

                ChromaController.BeatmapObjectManager.noteWasCutEvent -= NoteColorManager.ColorizeSaber;
                ChromaController.BeatmapObjectManager.noteWasCutEvent += NoteColorManager.ColorizeSaber;
            }

            NoteColorManager.EnableNoteColorOverride(noteController);
        }

        private static void Postfix()
        {
            NoteColorManager.DisableNoteColorOverride();
        }
    }
}
