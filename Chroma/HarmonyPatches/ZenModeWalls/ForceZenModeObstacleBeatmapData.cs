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

            __result = beatmapData.GetFilteredCopy(item => item is not BeatmapObjectData or WaypointData or ObstacleData ? item : null);

            return false;
        }
    }
}
