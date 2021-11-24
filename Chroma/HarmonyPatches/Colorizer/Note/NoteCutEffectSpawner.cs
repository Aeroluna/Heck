using HarmonyLib;
using JetBrains.Annotations;

namespace Chroma.HarmonyPatches.Colorizer.Note
{
    [HarmonyPatch(typeof(NoteCutCoreEffectsSpawner))]
    [HarmonyPatch("SpawnNoteCutEffect")]
    internal static class NoteCutEffectSpawnerSpawnNoteCutEffect
    {
        [HarmonyPriority(Priority.Low)]
        private static void Prefix(NoteController noteController)
        {
            ColorManagerColorForType.EnableColorOverride(noteController);
        }

        [UsedImplicitly]
        private static void Postfix()
        {
            ColorManagerColorForType.DisableColorOverride();
        }
    }
}
