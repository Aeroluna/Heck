namespace Chroma.HarmonyPatches
{
    [ChromaPatch(typeof(BeatmapObjectSpawnController))]
    [ChromaPatch("Start")]
    internal static class BeatmapObjectSpawnControllerStart
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(BeatmapObjectSpawnController __instance)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            __instance.StartCoroutine(ChromaController.DelayedStart(__instance));
        }
    }
}
