using Chroma.Colorizer;
using HarmonyLib;

namespace Chroma.HarmonyPatches.Colorizer.Initialize
{
    [HarmonyPatch(typeof(ObstacleDissolve))]
    [HarmonyPatch("Awake")]
    internal static class ObstacleDissolveAwake
    {
        [HarmonyPriority(Priority.High)]
        private static void Prefix(ObstacleControllerBase ____obstacleController)
        {
            ObstacleColorizer.Create(____obstacleController);
        }
    }

    [HarmonyPatch(typeof(ObstacleDissolve))]
    [HarmonyPatch("OnDestroy")]
    internal static class ObstacleDissolveOnDestroy
    {
        [HarmonyPriority(Priority.Low)]
        private static void Postfix(ObstacleControllerBase ____obstacleController)
        {
            ObstacleColorizer.Colorizers.Remove(____obstacleController);
        }
    }
}
