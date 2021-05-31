namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using Heck;

    [HeckPatch(typeof(MirroredObstacleController))]
    [HeckPatch("UpdatePositionAndRotation")]
    internal static class MirroredObstacleControllerUpdatePositionAndRotation
    {
        private static void Postfix(MirroredObstacleController __instance, ObstacleController ____followedObstacle)
        {
            if (____followedObstacle.TryGetObstacleColorizer(out ObstacleColorizer obstacleColorizer))
            {
                __instance.ColorizeObstacle(obstacleColorizer.Color);
            }
        }
    }
}
