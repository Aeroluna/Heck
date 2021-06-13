namespace Heck.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using Heck.Animation;
    using static Heck.Plugin;

    [HarmonyPatch(typeof(BeatmapDataLoader))]
    [HarmonyPatch("GetBeatmapDataFromBeatmapSaveData")]
    internal static class BeatmapDataLoaderGetBeatmapDataFromBeatmapSaveData
    {
        private static void Postfix(BeatmapData __result)
        {
            if (__result is CustomBeatmapData customBeatmapData)
            {
                TrackBuilder trackManager = new TrackBuilder(customBeatmapData);
                foreach (BeatmapLineData beatmapLineData in customBeatmapData.beatmapLinesData)
                {
                    foreach (BeatmapObjectData beatmapObjectData in beatmapLineData.beatmapObjectsData)
                    {
                        Dictionary<string, object> dynData;
                        switch (beatmapObjectData)
                        {
                            case CustomObstacleData obstacleData:
                                dynData = obstacleData.customData;
                                break;

                            case CustomNoteData noteData:
                                dynData = noteData.customData;
                                break;

                            default:
                                continue;
                        }

                        // for epic tracks thing
                        string trackName = dynData.Get<string>(TRACK);
                        if (trackName != null)
                        {
                            dynData["track"] = trackManager.AddTrack(trackName);
                        }
                    }
                }

                customBeatmapData.customData["tracks"] = trackManager.Tracks;

                IEnumerable<Dictionary<string, object>> pointDefinitions = customBeatmapData.customData.Get<List<object>>(POINTDEFINITIONS)?.Cast<Dictionary<string, object>>();
                if (pointDefinitions == null)
                {
                    return;
                }

                PointDefinitionBuilder pointDataManager = new PointDefinitionBuilder();
                foreach (Dictionary<string, object> pointDefintion in pointDefinitions)
                {
                    string pointName = pointDefintion.Get<string>(NAME);
                    PointDefinition pointData = PointDefinition.ListToPointData(pointDefintion.Get<List<object>>(POINTS));
                    pointDataManager.AddPoint(pointName, pointData);
                }

                customBeatmapData.customData["pointDefinitions"] = pointDataManager.PointData;
            }
        }
    }
}
