namespace Chroma.HarmonyPatches
{
    using Chroma.Events;
    using Chroma.Settings;
    using Chroma.Utils;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("HandleNoteControllerDidInitEvent")]
    internal class ColorNoteVisualsHandleNoteControllerDidInitEvent
    {
        internal static bool NoteColoursActive { get; set; }

#pragma warning disable SA1313
        private static void Prefix(NoteController noteController, ColorManager ____colorManager)
#pragma warning restore SA1313
        {
            NoteData noteData = noteController.noteData;
            bool warm = noteData.noteType == NoteType.NoteA;
            Color? c = null;

            // CustomJSONData _customData individual color override
            if (noteData is CustomNoteData customData && ChromaBehaviour.LightingRegistered && ChromaConfig.Instance.NoteColorEventsEnabled)
            {
                dynamic dynData = customData.customData;
                c = ChromaUtils.GetColorFromData(dynData, false) ?? c;
            }

            if (c.HasValue)
            {
                ChromaColorManager.SetNoteTypeColourOverride(noteData.noteType, c.Value);
                NoteColoursActive = true;
            }

            if (NoteColoursActive)
            {
                ChromaNoteColourEvent.SavedNoteColours[noteController] = ____colorManager.ColorForNoteType(noteData.noteType);
                noteController.noteWasCutEvent += ChromaNoteColourEvent.SaberColour;
            }
        }

        private static void Postfix(ref NoteController noteController)
        {
            ChromaColorManager.RemoveNoteTypeColourOverride(noteController.noteData.noteType);
        }
    }
}
