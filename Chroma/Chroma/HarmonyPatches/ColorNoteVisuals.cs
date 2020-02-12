using Chroma.Beatmap.Events;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
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
        private static void Prefix(ref NoteController noteController)
        {
            NoteData noteData = noteController.noteData;
            bool warm = noteData.noteType == NoteType.NoteA;
            Color? c = warm ? ColourManager.A : ColourManager.B;

            // Technicolour
            if (ColourManager.TechnicolourBlocks && ChromaConfig.TechnicolourBlocksStyle != ColourManager.TechnicolourStyle.GRADIENT)
            {
                try
                {
                    c = ColourManager.GetTechnicolour(noteData, ChromaConfig.TechnicolourBlocksStyle);
                }
                catch (Exception e)
                {
                    ChromaLogger.Log(e);
                }
            }

            // CustomLightColours
            if (ChromaNoteColourEvent.CustomNoteColours.Count > 0)
            {
                Dictionary<float, Color> dictionaryID;
                if (ChromaNoteColourEvent.CustomNoteColours.TryGetValue(noteData.noteType, out dictionaryID))
                {
                    foreach (KeyValuePair<float, Color> d in dictionaryID)
                    {
                        if (d.Key <= noteData.time)
                        {
                            c = d.Value;
                        }
                    }
                }
            }

            // CustomJSONData _customData individual color override
            try
            {
                if (noteData is CustomNoteData customData && ChromaBehaviour.LightingRegistered && ChromaConfig.NoteColourEventsEnabled)
                {
                    dynamic dynData = customData.customData;
                    if (dynData != null)
                    {
                        float? r = (float?)Trees.at(dynData, "_r");
                        float? g = (float?)Trees.at(dynData, "_g");
                        float? b = (float?)Trees.at(dynData, "_b");
                        if (r != null && g != null && b != null)
                        {
                            c = new Color(r.Value, g.Value, b.Value);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ChromaLogger.Log("INVALID _customData", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            if (c != null)
            {
                ChromaNoteColourEvent.SavedNoteColours.Add(noteController, (Color)c);
                ColourManager.SetNoteTypeColourOverride(noteData.noteType, (Color)c);
            }

            // colour sabers to color of block we smack
            if (ChromaNoteColourEvent.SavedNoteColours.Count > 0)
            {
                noteController.noteWasCutEvent += ChromaNoteColourEvent.SaberColour;
            }
        }

        private static void Postfix(ref NoteController noteController)
        {
            ColourManager.RemoveNoteTypeColourOverride(noteController.noteData.noteType);
        }
    }
}