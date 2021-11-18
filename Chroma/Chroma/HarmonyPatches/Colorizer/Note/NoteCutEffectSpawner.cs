namespace Chroma.HarmonyPatches
{
    using HarmonyLib;

    [HarmonyPatch(typeof(NoteCutCoreEffectsSpawner))]
    [HarmonyPatch("SpawnNoteCutEffect")]
    internal static class NoteCutEffectSpawnerSpawnNoteCutEffect
    {
        [HarmonyPriority(Priority.Low)]
        private static void Prefix(NoteController noteController)
        {
            ColorManagerColorForType.EnableColorOverride(noteController);
        }

        private static void Postfix()
        {
            ColorManagerColorForType.DisableColorOverride();
        }
    }
}
