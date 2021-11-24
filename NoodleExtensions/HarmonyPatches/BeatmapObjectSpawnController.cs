using Heck;
using JetBrains.Annotations;

namespace NoodleExtensions.HarmonyPatches
{
    [HeckPatch(typeof(BeatmapObjectSpawnController))]
    [HeckPatch("Start")]
    internal static class BeatmapObjectSpawnControllerStart
    {
        [UsedImplicitly]
        private static void Postfix(IBeatmapObjectSpawner ____beatmapObjectSpawner, BeatmapObjectSpawnMovementData ____beatmapObjectSpawnMovementData)
        {
            if (____beatmapObjectSpawner is BasicBeatmapObjectManager)
            {
                SpawnDataHelper.InitBeatmapObjectSpawnController(____beatmapObjectSpawnMovementData);
            }
        }
    }
}
