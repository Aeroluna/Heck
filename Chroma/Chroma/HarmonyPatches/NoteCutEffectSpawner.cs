namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;

    [HarmonyPatch(typeof(NoteCutCoreEffectsSpawner))]
    [HarmonyPatch("SpawnNoteCutEffect")]
    internal static class NoteCutEffectSpawnerSpawnNoteCutEffect
    {
        [HarmonyPriority(Priority.Low)]
        private static void Prefix(NoteController noteController)
        {
            NoteColorizer.EnableNoteColorOverride(noteController);
        }

        private static void Postfix()
        {
            NoteColorizer.DisableNoteColorOverride();
        }
    }
}
