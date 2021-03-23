namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(BeatmapObjectSpawnController))]
    [NoodlePatch("Start")]
    internal static class BeatmapObjectSpawnControllerStart
    {
        internal static BeatmapObjectSpawnController BeatmapObjectSpawnController { get; private set; }

        internal static BasicBeatmapObjectManager BeatmapObjectManager { get; private set; }

        private static void Postfix(BeatmapObjectSpawnController __instance, IBeatmapObjectSpawner ____beatmapObjectSpawner, BeatmapObjectSpawnMovementData ____beatmapObjectSpawnMovementData)
        {
            BeatmapObjectSpawnController = __instance;

            if (____beatmapObjectSpawner is BasicBeatmapObjectManager basicBeatmapObjectManager)
            {
                BeatmapObjectManager = basicBeatmapObjectManager;
                SpawnDataHelper.InitBeatmapObjectSpawnController(____beatmapObjectSpawnMovementData);
            }
        }
    }
}
