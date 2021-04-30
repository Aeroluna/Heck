namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Collections.Generic;
    using HarmonyLib;
    using Heck;

    [HeckPatch(typeof(BasicBeatmapObjectManager))]
    [HeckPatch(MethodType.Constructor)]
    [HeckPatch(new Type[] { typeof(BasicBeatmapObjectManager.InitData), typeof(GameNoteController.Pool), typeof(BombNoteController.Pool), typeof(ObstacleController.Pool) })]
    internal static class BasicBeatmapObjectManagerSpawnObstacleInternal
    {
        private static void Postfix()
        {
            ObstacleControllerInit._activeObstacles.Clear();
        }
    }

    [HeckPatch(typeof(BasicBeatmapObjectManager))]
    [HeckPatch("get_activeObstacleControllers")]
    internal static class BasicBeatmapObjectManagerGetActiveObstacleControllers
    {
        private static bool Prefix(ref List<ObstacleController> __result)
        {
            __result = ObstacleControllerInit._activeObstacles;
            return false;
        }
    }

    [HeckPatch(typeof(BasicBeatmapObjectManager))]
    [HeckPatch("DespawnInternal")]
    [HeckPatch(new Type[] { typeof(ObstacleController) })]
    internal static class BasicBeatmapObjectManagerDespawnInternal
    {
        private static void Postfix(ObstacleController obstacleController)
        {
            ObstacleControllerInit._activeObstacles.Remove(obstacleController);
        }
    }
}
