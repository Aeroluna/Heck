namespace Chroma.HarmonyPatches
{
    using Chroma.Events;
    using Chroma.Utils;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("HandleNoteControllerDidInitEvent")]
    internal class ColorNoteVisualsHandleNoteControllerDidInitEvent
    {
        internal static bool NoteColorsActive { get; set; }

#pragma warning disable SA1313
        private static void Prefix(NoteController noteController, ColorManager ____colorManager)
#pragma warning restore SA1313
        {
            NoteData noteData = noteController.noteData;

            // CustomJSONData _customData individual color override
            if (noteData is CustomNoteData customData && ChromaBehaviour.LightingRegistered)
            {
                dynamic dynData = customData.customData;
                Color? c = ChromaUtils.GetColorFromData(dynData, false);

                if (c.HasValue)
                {
                    ChromaColorManager.SetNoteTypeColorOverride(noteData.noteType, c.Value);
                    NoteColorsActive = true;
                }

                if (NoteColorsActive)
                {
                    ChromaNoteColorEvent.SavedNoteColors[noteController] = ____colorManager.ColorForNoteType(noteData.noteType);
                    noteController.noteWasCutEvent += ChromaNoteColorEvent.SaberColor;
                    dynData.isSubscribed = true;
                }
            }
        }

        private static void Postfix(ref NoteController noteController)
        {
            ChromaColorManager.RemoveNoteTypeColorOverride(noteController.noteData.noteType);
        }
    }
}
