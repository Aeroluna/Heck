using Chroma.Events;
using Chroma.Settings;
using HarmonyLib;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatEffectSpawner))]
    [HarmonyPatch("HandleNoteDidStartJumpEvent")]
    internal class HandleNoteDidStartJumpEvent
    {
        private static void Prefix(NoteController noteController)
        {
            if (!ColourManager.TechnicolourBlocks || ChromaConfig.TechnicolourBlocksStyle != ColourManager.TechnicolourStyle.GRADIENT)
            {
                if (ChromaNoteColourEvent.SavedNoteColours.TryGetValue(noteController, out Color c))
                {
                    ColourManager.SetNoteTypeColourOverride(noteController.noteData.noteType, c);
                }
            }
        }

        private static void Postfix(NoteController noteController)
        {
            ColourManager.RemoveNoteTypeColourOverride(noteController.noteData.noteType);
        }
    }
}