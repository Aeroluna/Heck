namespace Chroma.HarmonyPatches
{
    using System.Collections.Generic;
    using HarmonyLib;

    [HarmonyPatch(typeof(PlayerHeadAndObstacleInteraction))]
    [HarmonyPatch("get_intersectingObstacles")]
    internal static class PlayerHeadAndObstacleInteractionIntersectingObstaclesGetter
    {
        private static readonly List<ObstacleController> _emptyList = new List<ObstacleController>();

        private static bool Prefix(ref List<ObstacleController> __result)
        {
            if (Settings.ChromaConfig.Instance.ForceZenWallsEnabled && GameplayCoreInstallerInstallBindings.ZenModeActive)
            {
                __result = _emptyList;
                return false;
            }

            return true;
        }
    }
}
