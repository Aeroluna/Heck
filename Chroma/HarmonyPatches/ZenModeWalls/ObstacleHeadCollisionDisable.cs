using Chroma.Settings;
using SiraUtil.Affinity;
using Zenject;

namespace Chroma.HarmonyPatches.ZenModeWalls
{
    internal class ObstacleHeadCollisionDisable : IAffinity
    {
        private readonly bool _zenMode;

        private ObstacleHeadCollisionDisable([Inject(Optional = true, Id = "zenMode")] bool zenMode)
        {
            _zenMode = zenMode;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(PlayerHeadAndObstacleInteraction), "RefreshIntersectingObstacles")]
        private bool Prefix()
        {
            return !ChromaConfig.Instance.ForceZenWallsEnabled ||
                   !_zenMode;
        }
    }
}
