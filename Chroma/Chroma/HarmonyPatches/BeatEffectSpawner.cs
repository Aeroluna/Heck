using Chroma.Events;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatEffectSpawner))]
    [HarmonyPatch("HandleNoteDidStartJumpEvent")]
    internal class HandleNoteDidStartJumpEvent
    {
        private static bool Prefix(NoteController noteController)
        {
            try
            {
                if (ChromaBehaviour.LightingRegistered && noteController.noteData is CustomNoteData customData)
                {
                    dynamic dynData = customData.customData;
                    bool? reset = Trees.at(dynData, "_disableSpawnEffect");
                    if (reset.HasValue && reset == true) return false;
                }
            }
            catch (Exception e)
            {
                Logger.Log("INVALID _customData", Logger.Level.WARNING);
                Logger.Log(e);
            }

            if (!ColourManager.TechnicolourBlocks || ChromaConfig.TechnicolourBlocksStyle != ColourManager.TechnicolourStyle.GRADIENT)
            {
                if (ChromaNoteColourEvent.SavedNoteColours.TryGetValue(noteController, out Color c))
                {
                    ColourManager.SetNoteTypeColourOverride(noteController.noteData.noteType, c);
                }
            }
            return true;
        }

        private static void Postfix(NoteController noteController)
        {
            ColourManager.RemoveNoteTypeColourOverride(noteController.noteData.noteType);
        }
    }
}