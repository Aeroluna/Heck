namespace Chroma.HarmonyPatches
{
    [ChromaPatch(typeof(BeatmapObjectSpawnController))]
    [ChromaPatch("Start")]
    internal static class BeatmapObjectSpawnControllerStart
    {
        internal static BeatmapObjectSpawnController BeatmapObjectSpawnController { get; private set; }

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(BeatmapObjectSpawnController __instance)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            BeatmapObjectSpawnController = __instance;
        }
    }
}