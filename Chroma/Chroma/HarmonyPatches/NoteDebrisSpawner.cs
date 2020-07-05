namespace Chroma.HarmonyPatches
{
    using Chroma.Events;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(NoteDebrisSpawner))]
    [HarmonyPatch("SpawnDebris")]
    internal class NoteDebrisSpawnerSpawnDebris
    {
        private static void Prefix(INoteController noteController)
        {
            if (ChromaNoteColorEvent.SavedNoteColours.TryGetValue(noteController, out Color c))
            {
                ChromaColorManager.SetNoteTypeColourOverride(noteController.noteData.noteType, c);
            }
        }

        private static void Postfix(INoteController noteController)
        {
            ChromaColorManager.RemoveNoteTypeColourOverride(noteController.noteData.noteType);
        }
    }
}
