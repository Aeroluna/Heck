namespace NoodleExtensions.HarmonyPatches
{
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
                        dynamic customData;
                        if (beatmapObjectData is CustomObstacleData || beatmapObjectData is CustomNoteData || beatmapObjectData is WaypointData)
                        {
                            customData = beatmapObjectData;
                        }
                        else
                        {
                            continue;
                        }

                        dynamic dynData = customData.customData;

                        // TODO: account for base game bpm changes
                        // for per object njs and spawn offset
                        float bpm = startBpm;
                        dynData.bpm = bpm;
                    }
                }
            }
        }
    }
}
