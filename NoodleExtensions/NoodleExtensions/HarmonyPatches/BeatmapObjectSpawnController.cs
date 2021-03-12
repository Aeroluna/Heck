namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(BeatmapObjectSpawnController))]
    [NoodlePatch("Start")]
    internal static class BeatmapObjectSpawnControllerStart
    {
        internal static BeatmapObjectSpawnController BeatmapObjectSpawnController { get; private set; }

        private static void Postfix(BeatmapObjectSpawnController __instance)
        {
            BeatmapObjectSpawnController = __instance;
        }
    }
}
