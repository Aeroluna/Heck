using Chroma.Events;
using Chroma.Settings;
using Chroma.Utils;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("Awake")]
    internal class ColorNoteVisualsAwake
    {
        private static void Postfix(ColorNoteVisuals __instance)
        {
            if (VFX.TechnicolourController.Instantiated())
                VFX.TechnicolourController.Instance._colorNoteVisuals.Add(__instance);
        }
    }

    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("OnDestroy")]
    internal class ColorNoteVisualsOnDestroy
    {
        private static void Postfix(ColorNoteVisuals __instance)
        {
            if (VFX.TechnicolourController.Instantiated())
                VFX.TechnicolourController.Instance._colorNoteVisuals.Remove(__instance);
        }
    }

    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("HandleNoteControllerDidInitEvent")]
    internal class ColorNoteVisualsHandleNoteControllerDidInitEvent
    {
        internal static bool noteColoursActive;

        private static void Prefix(NoteController noteController, ColorManager ____colorManager)
        {
            NoteData noteData = noteController.noteData;
            bool warm = noteData.noteType == NoteType.NoteA;
            Color? c = null;

            // Technicolour
            if (ColourManager.TechnicolourBlocks && ChromaConfig.TechnicolourBlocksStyle != ColourManager.TechnicolourStyle.GRADIENT)
            {
                c = ColourManager.GetTechnicolour(noteData.noteType == NoteType.NoteA, noteData.time + noteData.lineIndex + (int)noteData.noteLineLayer, ChromaConfig.TechnicolourBlocksStyle);
            }

            // CustomLightColours
            if (ChromaNoteColourEvent.NoteColours.TryGetValue(noteData.noteType, out List<TimedColor> dictionaryID))
            {
                List<TimedColor> colors = dictionaryID.Where(n => n.time <= noteData.time).ToList();
                if (colors.Count > 0) c = colors.Last().color;
            }

            // CustomJSONData _customData individual color override
            try
            {
                if (noteData is CustomNoteData customData && ChromaBehaviour.LightingRegistered && ChromaConfig.NoteColourEventsEnabled)
                {
                    dynamic dynData = customData.customData;
                    c = ChromaUtils.GetColorFromData(dynData, false) ?? c;
                }
            }
            catch (Exception e)
            {
                Logger.Log("INVALID _customData", Logger.Level.WARNING);
                Logger.Log(e);
            }

            if (c.HasValue)
            {
                ColourManager.SetNoteTypeColourOverride(noteData.noteType, c.Value);
                noteColoursActive = true;
            }

            if (noteColoursActive || (ColourManager.TechnicolourBlocks && ChromaConfig.TechnicolourBlocksStyle == ColourManager.TechnicolourStyle.GRADIENT))
            {
                ChromaNoteColourEvent.SavedNoteColours[noteController] = ____colorManager.ColorForNoteType(noteData.noteType);
                if (!ColourManager.TechnicolourSabers) noteController.noteWasCutEvent += ChromaNoteColourEvent.SaberColour;
            }
        }

        private static void Postfix(ref NoteController noteController)
        {
            ColourManager.RemoveNoteTypeColourOverride(noteController.noteData.noteType);
        }
    }
}