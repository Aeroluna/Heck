using Chroma.Events;
using Chroma.Settings;
using Chroma.Utils;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("Awake")]
    internal class ColorNoteVisualsAwake
    {
        private static void Postfix(ColorNoteVisuals __instance)
        {
            if (ColourManager.TechnicolourBlocks && ChromaConfig.TechnicolourBlocksStyle == ColourManager.TechnicolourStyle.GRADIENT)
                VFX.TechnicolourController.Instance._colorNoteVisuals.Add(__instance);
        }
    }

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("HandleNoteControllerDidInitEvent")]
    internal class ColorNoteVisualsHandleNoteControllerDidInitEvent
    {
        internal static bool noteColoursActive;

        private static void Prefix(ref NoteController noteController, ref ColorManager ____colorManager)
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
            if (ChromaNoteColourEvent.CustomNoteColours.Count > 0)
            {
                if (ChromaNoteColourEvent.CustomNoteColours.TryGetValue(noteData.noteType, out Dictionary<float, Color> dictionaryID))
                {
                    foreach (KeyValuePair<float, Color> d in dictionaryID)
                    {
                        if (d.Key <= noteData.time) c = d.Value;
                    }
                }
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
                ChromaLogger.Log("INVALID _customData", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            if (c.HasValue)
            {
                ColourManager.SetNoteTypeColourOverride(noteData.noteType, c.Value);
                noteColoursActive = true;
            }

            if (noteColoursActive || ChromaConfig.TechnicolourBlocksStyle == ColourManager.TechnicolourStyle.GRADIENT)
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