using System.Collections.Generic;
using Chroma.Settings;
using HarmonyLib;
using JetBrains.Annotations;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(PlayerHeadAndObstacleInteraction))]
    [HarmonyPatch("get_intersectingObstacles")]
    internal static class PlayerHeadAndObstacleInteractionIntersectingObstaclesGetter
    {
        private static readonly List<ObstacleController> _emptyList = new();

        [UsedImplicitly]
        private static bool Prefix(ref List<ObstacleController> __result)
        {
            if (!ChromaConfig.Instance.ForceZenWallsEnabled ||
                !GameplayCoreInstallerInstallBindings.ZenModeActive)
            {
                return true;
            }

            __result = _emptyList;
            return false;
        }
    }
}
