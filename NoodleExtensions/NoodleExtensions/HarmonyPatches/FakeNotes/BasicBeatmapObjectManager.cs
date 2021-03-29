namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Collections.Generic;
    using HarmonyLib;

    // Too lazy to make noodle patches support constructors
    [HarmonyPatch(typeof(BasicBeatmapObjectManager))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(BasicBeatmapObjectManager.InitData), typeof(GameNoteController.Pool), typeof(BombNoteController.Pool), typeof(ObstacleController.Pool) })]
    internal static class BasicBeatmapObjectManagerCtor
    {
        private static void Postfix()
        {
            ObstacleControllerInit._activeObstacles.Clear();
        }
    }

    [NoodlePatch(typeof(BasicBeatmapObjectManager))]
    [NoodlePatch("get_activeObstacleControllers")]
    internal static class BasicBeatmapObjectManagerGetActiveObstacleControllers
    {
        private static bool Prefix(ref List<ObstacleController> __result)
        {
            __result = ObstacleControllerInit._activeObstacles;
            return false;
        }
    }

    [NoodlePatch(typeof(BasicBeatmapObjectManager))]
    [NoodlePatch("DespawnInternal")]
    [NoodlePatch(new Type[] { typeof(ObstacleController) })]
    internal static class BasicBeatmapObjectManagerDespawnInternal
    {
        private static void Postfix(ObstacleController obstacleController)
        {
            ObstacleControllerInit._activeObstacles.Remove(obstacleController);
        }
    }
}
