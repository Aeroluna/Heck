using Chroma.Colorizer;
using Heck;
using JetBrains.Annotations;

namespace Chroma.HarmonyPatches.Mirror
{
    [HeckPatch(typeof(MirroredObstacleController))]
    [HeckPatch("UpdatePositionAndRotation")]
    internal static class MirroredObstacleControllerUpdatePositionAndRotation
    {
        [UsedImplicitly]
        private static void Postfix(MirroredObstacleController __instance, ObstacleController ____followedObstacle)
        {
            __instance.ColorizeObstacle(____followedObstacle.GetObstacleColorizer().Color);
        }
    }
}
