using Chroma.Settings;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chroma.Beatmap.Events;
using UnityEngine;

namespace Chroma.HarmonyPatches {
    
    [HarmonyPatch(typeof(NoteDebrisSpawner))]
    [HarmonyPatch("SpawnDebris")]
    class NoteDebrisSpawnerSpawnDebris {

        static void Prefix(ref INoteController noteController) {
            if (!ColourManager.TechnicolourBlocks || ChromaConfig.TechnicolourBlocksStyle != ColourManager.TechnicolourStyle.GRADIENT) {
                if (ChromaNoteColourEvent.SavedNoteColours.TryGetValue(noteController, out Color c)) {
                    ColourManager.SetNoteTypeColourOverride(noteController.noteData.noteType, c);
                }
            }
        }

        static void Postfix(ref INoteController noteController) {
            ColourManager.RemoveNoteTypeColourOverride(noteController.noteData.noteType);
        }

    }

}