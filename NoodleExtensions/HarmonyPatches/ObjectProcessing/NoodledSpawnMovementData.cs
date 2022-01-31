using NoodleExtensions.Managers;
using SiraUtil.Affinity;

namespace NoodleExtensions.HarmonyPatches.ObjectProcessing
{
    internal class NoodledSpawnMovementData : IAffinity
    {
        private readonly SpawnDataManager _spawnDataManager;

        private NoodledSpawnMovementData(SpawnDataManager spawnDataManager)
        {
            _spawnDataManager = spawnDataManager;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapObjectSpawnMovementData), nameof(BeatmapObjectSpawnMovementData.GetObstacleSpawnData))]
        private bool ObstacleNoodlePatch(ObstacleData obstacleData, ref BeatmapObjectSpawnMovementData.ObstacleSpawnData __result)
        {
            return _spawnDataManager.GetObstacleSpawnData(obstacleData, ref __result);
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapObjectSpawnMovementData), nameof(BeatmapObjectSpawnMovementData.GetJumpingNoteSpawnData))]
        private bool NoteNoodlePatch(BeatmapObjectSpawnMovementData __instance, NoteData noteData, ref BeatmapObjectSpawnMovementData.NoteSpawnData __result)
        {
            return _spawnDataManager.GetJumpingNoteSpawnData(noteData, ref __result);
        }
    }
}
