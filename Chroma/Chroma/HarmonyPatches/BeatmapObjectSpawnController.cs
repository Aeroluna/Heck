namespace Chroma.HarmonyPatches
{
    using HarmonyLib;

    [HarmonyPatch(typeof(BeatmapObjectSpawnController))]
    [HarmonyPatch("Start")]
    internal static class BeatmapObjectSpawnControllerStart
    {
        private static void Postfix(BeatmapObjectSpawnController __instance)
        {
            __instance.StartCoroutine(ChromaController.DelayedStart(__instance));
        }
    }
}
