namespace Chroma.HarmonyPatches
{
    using Chroma.Events;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(NoteCutEffectSpawner))]
    [HarmonyPatch("SpawnNoteCutEffect")]
    internal class NoteCutEffectSpawnerSpawnNoteCutEffect
    {
        private static void Prefix(NoteController noteController)
        {
            if (ChromaNoteColourEvent.SavedNoteColours.TryGetValue(noteController, out Color c))
            {
                ChromaColorManager.SetNoteTypeColourOverride(noteController.noteData.noteType, c);
            }
        }

        private static void Postfix(NoteController noteController)
        {
            ChromaColorManager.RemoveNoteTypeColourOverride(noteController.noteData.noteType);
        }
    }
}
