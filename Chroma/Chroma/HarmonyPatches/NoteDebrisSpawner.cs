using Chroma.Events;
using Chroma.Settings;
using HarmonyLib;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteDebrisSpawner))]
    [HarmonyPatch("SpawnDebris")]
    internal class NoteDebrisSpawnerSpawnDebris
    {
        private static void Prefix(INoteController noteController)
        {
            if (!ColourManager.TechnicolourBlocks || ChromaConfig.TechnicolourBlocksStyle != ColourManager.TechnicolourStyle.GRADIENT)
            {
                if (ChromaNoteColourEvent.SavedNoteColours.TryGetValue(noteController, out Color c))
                {
                    ColourManager.SetNoteTypeColourOverride(noteController.noteData.noteType, c);
                }
            }
        }

        private static void Postfix(INoteController noteController)
        {
            ColourManager.RemoveNoteTypeColourOverride(noteController.noteData.noteType);
        }
    }
}