namespace NoodleExtensions.HarmonyPatches
{
    using Heck;

    [HeckPatch(typeof(BeatmapObjectSpawnController))]
    [HeckPatch("Start")]
    internal static class BeatmapObjectSpawnControllerStart
    {
        internal static BasicBeatmapObjectManager BeatmapObjectManager { get; private set; }

        private static void Postfix(BeatmapObjectSpawnController __instance, IBeatmapObjectSpawner ____beatmapObjectSpawner, BeatmapObjectSpawnMovementData ____beatmapObjectSpawnMovementData)
        {
            if (____beatmapObjectSpawner is BasicBeatmapObjectManager basicBeatmapObjectManager)
            {
                BeatmapObjectManager = basicBeatmapObjectManager;
                SpawnDataHelper.InitBeatmapObjectSpawnController(____beatmapObjectSpawnMovementData);
            }
        }
    }
}
