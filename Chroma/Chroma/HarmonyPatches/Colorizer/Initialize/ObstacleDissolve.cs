namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;

    [HarmonyPatch(typeof(ObstacleDissolve))]
    [HarmonyPatch("Awake")]
    internal static class ObstacleDissolveAwake
    {
        [HarmonyPriority(Priority.High)]
        private static void Prefix(ObstacleControllerBase ____obstacleController)
        {
            new ObstacleColorizer(____obstacleController);
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
