using Chroma.Settings;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("HandleNoteControllerDidInitEvent")]
    class ColorNoteVisualsHandleNoteControllerDidInitEvent {

        static void Prefix(ref NoteController noteController) {
            if (ColourManager.TechnicolourBlocks && ChromaConfig.TechnicolourBlocksStyle != ColourManager.TechnicolourStyle.GRADIENT) {
                try {
                    ColourManager.SetNoteTypeColourOverride(noteController.noteData.noteType, ColourManager.GetTechnicolour(noteController.noteData, ChromaConfig.TechnicolourBlocksStyle));
                } catch (Exception e) {
                    ChromaLogger.Log(e);
                }
            }
        }

        static void Postfix(ref NoteController noteController) {
            ColourManager.RemoveNoteTypeColourOverride(noteController.noteData.noteType);
        }

    }

}
