namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;

    [HarmonyPatch(typeof(BeatmapDataLoader))]
    [HarmonyPatch("GetBeatmapDataFromBeatmapSaveData")]
    internal static class BeatmapDataLoaderGetBeatmapDataFromBeatmapSaveData
    {
        private static void Postfix(BeatmapData __result, float startBpm)
        {
            if (__result is CustomBeatmapData customBeatmapData)
            {
                foreach (BeatmapLineData beatmapLineData in customBeatmapData.beatmapLinesData)
                {
                    foreach (BeatmapObjectData beatmapObjectData in beatmapLineData.beatmapObjectsData)
                    {
                        Dictionary<string, object?> dynData = beatmapObjectData.GetDataForObject();

                        // TODO: account for base game bpm changes
                        // for per object njs and spawn offset
                        float bpm = startBpm;
                        dynData["bpm"] = bpm;
                    }
                }
            }
        }
    }
}
