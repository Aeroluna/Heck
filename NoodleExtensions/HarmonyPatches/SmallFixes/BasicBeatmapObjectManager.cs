using System.Collections.Generic;
using Heck;
using JetBrains.Annotations;

namespace NoodleExtensions.HarmonyPatches.SmallFixes
{
    // Do not add fake obstacles to active obstacles to increase performance
    [HeckPatch(typeof(BasicBeatmapObjectManager))]
    [HeckPatch("Init")]
    internal static class BasicBeatmapObjectManagerInit
    {
        [UsedImplicitly]
        private static void Postfix()
        {
            ObstacleControllerInit._activeObstacles.Clear();
        }
    }

    [HeckPatch(typeof(BasicBeatmapObjectManager))]
    [HeckPatch("get_activeObstacleControllers")]
    internal static class BasicBeatmapObjectManagerGetActiveObstacleControllers
    {
        [UsedImplicitly]
        private static bool Prefix(ref List<ObstacleController> __result)
        {
            __result = ObstacleControllerInit._activeObstacles;
            return false;
        }
    }

    [HeckPatch(typeof(BasicBeatmapObjectManager))]
    [HeckPatch("DespawnInternal")]
    [HeckPatch(new[] { typeof(ObstacleController) })]
    internal static class BasicBeatmapObjectManagerDespawnInternal
    {
        [UsedImplicitly]
        private static void Postfix(ObstacleController obstacleController)
        {
            ObstacleControllerInit._activeObstacles.Remove(obstacleController);
        }
    }
}
