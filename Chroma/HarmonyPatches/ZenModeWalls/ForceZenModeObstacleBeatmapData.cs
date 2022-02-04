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

            BeatmapData copyWithoutBeatmapObjects = beatmapData.GetCopyWithoutBeatmapObjects();
            BeatmapData.CopyBeatmapObjectsWaypointsOnly(beatmapData, copyWithoutBeatmapObjects);
            foreach (BeatmapObjectData beatmapObjectData in beatmapData.beatmapObjectsData)
            {
                if (beatmapObjectData is ObstacleData)
                {
                    copyWithoutBeatmapObjects.AddBeatmapObjectData(beatmapObjectData.GetCopy());
                }
            }

            __result = copyWithoutBeatmapObjects;

            return false;
        }
    }
}
