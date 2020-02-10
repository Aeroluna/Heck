using Chroma.Beatmap.Events;
using Chroma.Misc;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("Awake")]
    class ColorNoteVisualsAwake {

        static void Postfix(ColorNoteVisuals __instance) {
            if (ColourManager.TechnicolourBlocks && ChromaConfig.TechnicolourBlocksStyle == ColourManager.TechnicolourStyle.GRADIENT)
                VFX.TechnicolourController.Instance._colorNoteVisuals.Add(__instance);
        }

    }

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("HandleNoteControllerDidInitEvent")]
    class ColorNoteVisualsHandleNoteControllerDidInitEvent {

        static void Prefix(ref NoteController noteController) {
            NoteData noteData = noteController.noteData;
            bool warm = noteData.noteType == NoteType.NoteA;
            Color? c = warm ? ColourManager.A : ColourManager.B;

            // Technicolour
            if (ColourManager.TechnicolourBlocks && ChromaConfig.TechnicolourBlocksStyle != ColourManager.TechnicolourStyle.GRADIENT) {
                try {
                    c = ColourManager.GetTechnicolour(noteData, ChromaConfig.TechnicolourBlocksStyle);
                }
                catch (Exception e) {
                    ChromaLogger.Log(e);
                }
            }

            // CustomLightColours
            if (ChromaNoteColourEvent.CustomNoteColours.Count > 0) {
                Dictionary<float, Color> dictionaryID;
                if (ChromaNoteColourEvent.CustomNoteColours.TryGetValue(noteData.noteType, out dictionaryID)) {
                    foreach (KeyValuePair<float, Color> d in dictionaryID) {
                        if (d.Key <= noteData.time) {
                            c = d.Value;
                        }
                    }
                }
            }

            // CustomJSONData _customData individual color override
            try {
                if (noteData is CustomNoteData customData && ChromaBehaviour.LightingRegistered) {
                    dynamic dynData = customData.customData;
                    if (dynData != null) {
                        float? r = (float?)Trees.at(dynData, "_noteR");
                        float? g = (float?)Trees.at(dynData, "_noteG");
                        float? b = (float?)Trees.at(dynData, "_noteB");
                        if (r != null && g != null && b != null) {
                            c = new Color(r.Value, g.Value, b.Value);
                        }
                    }
                }
            }
            catch (Exception e) {
                ChromaLogger.Log("INVALID _customData", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            if (c != null) {
                ChromaNoteColourEvent.SavedNoteColours.Add(noteController, (Color)c);
                ColourManager.SetNoteTypeColourOverride(noteData.noteType, (Color)c);
            }
        }

        static void Postfix(ref NoteController noteController) {
            ColourManager.RemoveNoteTypeColourOverride(noteController.noteData.noteType);
        }

    }

}
