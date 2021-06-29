namespace Chroma.HarmonyPatches
{
    using HarmonyLib;

    [HarmonyPatch(typeof(BeatmapDataZenModeTransform))]
    [HarmonyPatch("CreateTransformedData")]
    internal static class BeatmapDataZenModeTransformCreateTransformedData
    {
        private static bool Prefix(IReadonlyBeatmapData beatmapData, ref IReadonlyBeatmapData __result)
        {
            if (Settings.ChromaConfig.Instance!.ForceZenWallsEnabled)
            {
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

            return true;
        }
    }
}
