using Chroma.Settings;
using SiraUtil.Affinity;
using Zenject;

namespace Chroma.HarmonyPatches.ZenModeWalls
{
    internal class ObstacleHeadCollisionDisable : IAffinity
    {
        private readonly bool _zenMode;
        private readonly Config _config;

        private ObstacleHeadCollisionDisable(
            [Inject(Optional = true, Id = "zenMode")] bool zenMode,
            Config config)
        {
            _zenMode = zenMode;
            _config = config;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(PlayerHeadAndObstacleInteraction), "RefreshIntersectingObstacles")]
        private bool Prefix()
        {
            return !_config.ForceZenWallsEnabled ||
                   !_zenMode;
        }
    }
}
