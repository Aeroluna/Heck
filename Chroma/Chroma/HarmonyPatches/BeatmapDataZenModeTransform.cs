namespace Chroma.HarmonyPatches
{
    using HarmonyLib;

    [HarmonyPatch(typeof(BeatmapDataZenModeTransform))]
    [HarmonyPatch("CreateTransformedData")]
    internal static class BeatmapDataZenModeTransformCreateTransformedData
    {
        private static void Postfix(IReadonlyBeatmapData beatmapData, ref IReadonlyBeatmapData __result)
        {
            if (Settings.ChromaConfig.Instance.ForceZenWallsEnabled)
            {
                foreach (BeatmapObjectData beatmapObjectData in beatmapData.beatmapObjectsData)
                {
                    if (beatmapObjectData is ObstacleData)
                    {
                        ((BeatmapData)__result).AddBeatmapObjectData(beatmapObjectData.GetCopy());
                    }
                }
            }
        }
    }
}
