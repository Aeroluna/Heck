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
            if (ChromaNoteColorEvent.SavedNoteColors.TryGetValue(noteController, out Color c))
            {
                ChromaColorManager.SetNoteTypeColorOverride(noteController.noteData.noteType, c);
            }
        }

        private static void Postfix(INoteController noteController)
        {
            ChromaColorManager.RemoveNoteTypeColorOverride(noteController.noteData.noteType);
        }
    }
}
