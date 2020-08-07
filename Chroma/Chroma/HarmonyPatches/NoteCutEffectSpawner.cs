namespace Chroma.HarmonyPatches
{
    using Chroma.Events;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(NoteCutEffectSpawner))]
    [HarmonyPatch("SpawnNoteCutEffect")]
    internal class NoteCutEffectSpawnerSpawnNoteCutEffect
    {
        private static void Prefix(NoteController noteController)
        {
            NoteColorManager.EnableNoteColorOverride(noteController);
        }

        private static void Postfix()
        {
            NoteColorManager.DisableNoteColorOverride();
        }
    }
}
