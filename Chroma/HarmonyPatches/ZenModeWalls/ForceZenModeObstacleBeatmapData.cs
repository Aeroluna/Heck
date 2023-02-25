using Chroma.Settings;
using SiraUtil.Affinity;

namespace Chroma.HarmonyPatches.ZenModeWalls
{
    internal class ForceZenModeObstacleBeatmapData : IAffinity
    {
        private readonly Config _config;

        private ForceZenModeObstacleBeatmapData(Config config)
        {
            _config = config;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapDataZenModeTransform), "CreateTransformedData")]
        private bool Prefix(IReadonlyBeatmapData beatmapData, ref IReadonlyBeatmapData __result)
        {
            if (!_config.ForceZenWallsEnabled)
            {
                return true;
            }

            __result = beatmapData.GetFilteredCopy(item =>
            {
                return item switch
                {
                    WaypointData or ObstacleData => item,
                    BeatmapObjectData => null,
                    _ => item
                };
            });

            return false;
        }
    }
}
