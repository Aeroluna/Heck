using HarmonyLib;
using JetBrains.Annotations;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapObjectSpawnController))]
    [HarmonyPatch("Start")]
    internal static class BeatmapObjectSpawnControllerStart
    {
        [UsedImplicitly]
        private static void Postfix(BeatmapObjectSpawnController __instance)
        {
            __instance.StartCoroutine(ChromaController.DelayedStart(__instance));
        }
    }
}
