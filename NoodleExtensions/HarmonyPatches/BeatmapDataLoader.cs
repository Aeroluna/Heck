using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using JetBrains.Annotations;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapDataLoader))]
    [HarmonyPatch("GetBeatmapDataFromBeatmapSaveData")]
    internal static class BeatmapDataLoaderGetBeatmapDataFromBeatmapSaveData
    {
        [UsedImplicitly]
        private static void Postfix(BeatmapData __result, float startBpm)
        {
            if (__result is not CustomBeatmapData customBeatmapData)
            {
                return;
            }

            foreach (IReadonlyBeatmapLineData readonlyBeatmapLineData in customBeatmapData.beatmapLinesData)
            {
                BeatmapLineData beatmapLineData = (BeatmapLineData)readonlyBeatmapLineData;
                foreach (BeatmapObjectData beatmapObjectData in beatmapLineData.beatmapObjectsData)
                {
                    Dictionary<string, object?> dynData = beatmapObjectData.GetDataForObject();

                    // TODO: account for base game bpm changes
                    // for per object njs and spawn offset
                    dynData["bpm"] = startBpm;
                }
            }
        }
    }
}
