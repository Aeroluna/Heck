using System.Collections.Generic;
using Chroma.Settings;
using SiraUtil.Affinity;
using Zenject;

namespace Chroma.HarmonyPatches.ZenModeWalls
{
    internal class ObstacleHeadCollisionDisable : IAffinity
    {
        private static readonly List<ObstacleController> _emptyList = new();

        private readonly bool _zenMode;

        private ObstacleHeadCollisionDisable([Inject(Optional = true, Id = "zenMode")] bool zenMode)
        {
            _zenMode = zenMode;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(PlayerHeadAndObstacleInteraction), "get_intersectingObstacles")]
        private bool Prefix(ref List<ObstacleController> __result)
        {
            if (!ChromaConfig.Instance.ForceZenWallsEnabled ||
                !_zenMode)
            {
                return true;
            }

            __result = _emptyList;
            return false;
        }
    }
}
