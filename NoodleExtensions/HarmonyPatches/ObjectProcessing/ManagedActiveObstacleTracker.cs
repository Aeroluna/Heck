using System.Collections.Generic;
using SiraUtil.Affinity;

namespace NoodleExtensions.HarmonyPatches.ObjectProcessing
{
    // Do not add fake obstacles to active obstacles to increase performance
    internal class ManagedActiveObstacleTracker : IAffinity
    {
        private readonly List<ObstacleController> _activeObstacles = new();

        internal void AddActive(ObstacleController obstacleController)
        {
            _activeObstacles.Add(obstacleController);
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BasicBeatmapObjectManager), "get_activeObstacleControllers")]
        private bool ReturnManagedList(ref List<ObstacleController> __result)
        {
            __result = _activeObstacles;
            return false;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BasicBeatmapObjectManager), "DespawnInternal", AffinityMethodType.Normal, null, typeof(ObstacleController))]
        private void RemoveFromManagedList(ObstacleController obstacleController)
        {
            _activeObstacles.Remove(obstacleController);
        }
    }
}
