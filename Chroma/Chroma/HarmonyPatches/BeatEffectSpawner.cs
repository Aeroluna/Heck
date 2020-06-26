namespace Chroma.HarmonyPatches
{
    using Chroma.Events;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(BeatEffectSpawner))]
    [HarmonyPatch("HandleNoteDidStartJumpEvent")]
    internal class HandleNoteDidStartJumpEvent
    {
        private static bool Prefix(NoteController noteController)
        {
            if (ChromaBehaviour.LightingRegistered && noteController.noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                bool? reset = Trees.at(dynData, "_disableSpawnEffect");
                if (reset.HasValue && reset == true)
                {
                    return false;
                }
            }

            if (ChromaNoteColourEvent.SavedNoteColours.TryGetValue(noteController, out Color c))
            {
                ChromaColorManager.SetNoteTypeColourOverride(noteController.noteData.noteType, c);
            }

            return true;
        }

        private static void Postfix(NoteController noteController)
        {
            ChromaColorManager.RemoveNoteTypeColourOverride(noteController.noteData.noteType);
        }
    }
}
