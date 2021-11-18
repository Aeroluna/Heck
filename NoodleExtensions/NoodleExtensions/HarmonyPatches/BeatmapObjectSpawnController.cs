namespace NoodleExtensions.HarmonyPatches
{
    using Heck;

    [HeckPatch(typeof(BeatmapObjectSpawnController))]
    [HeckPatch("Start")]
    internal static class BeatmapObjectSpawnControllerStart
    {
        private static void Postfix(IBeatmapObjectSpawner ____beatmapObjectSpawner, BeatmapObjectSpawnMovementData ____beatmapObjectSpawnMovementData)
        {
            if (____beatmapObjectSpawner is BasicBeatmapObjectManager basicBeatmapObjectManager)
            {
                SpawnDataHelper.InitBeatmapObjectSpawnController(____beatmapObjectSpawnMovementData);
            }
        }
    }
}
