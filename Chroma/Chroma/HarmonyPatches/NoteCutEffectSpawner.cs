using Chroma.Settings;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma.HarmonyPatches {
    
    [HarmonyPatch(typeof(NoteCutEffectSpawner))]
    [HarmonyPatch("SpawnNoteCutEffect")]
    class NoteCutEffectSpawnerSpawnNoteCutEffect {

        static void Prefix(ref NoteController noteController) {
            if (ColourManager.TechnicolourBlocks) {
                ColourManager.SetNoteTypeColourOverride(noteController.noteData.noteType, ColourManager.GetTechnicolour(noteController.noteData.noteType == NoteType.NoteA, noteController.noteData.time, ChromaConfig.TechnicolourBlocksStyle));
            }
        }

        static void Postfix(ref NoteController noteController) {
            ColourManager.RemoveNoteTypeColourOverride(noteController.noteData.noteType);
        }

    }

}