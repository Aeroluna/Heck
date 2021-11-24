using Chroma.Settings;
using HarmonyLib;
using JetBrains.Annotations;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapDataZenModeTransform))]
    [HarmonyPatch("CreateTransformedData")]
    internal static class BeatmapDataZenModeTransformCreateTransformedData
    {
        [UsedImplicitly]
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
