using Chroma.Settings;
using HarmonyLib;
using Heck;

namespace Chroma.HarmonyPatches.ZenModeWalls
{
    [HeckPatch]
    [HarmonyPatch(typeof(BeatmapDataZenModeTransform))]
    internal static class ForceZenModeObstacleBeatmapData
    {
        [HarmonyPrefix]
        [HarmonyPatch("CreateTransformedData")]
        private static bool Prefix(IReadonlyBeatmapData beatmapData, ref IReadonlyBeatmapData __result)
        {
            if (!ChromaConfig.Instance.ForceZenWallsEnabled)
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
