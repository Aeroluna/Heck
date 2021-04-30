namespace Heck.HarmonyPatches
{
    using System.Collections.Generic;
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
                TrackManager trackManager = new TrackManager(customBeatmapData);
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

                        // for epic tracks thing
                        string trackName = Trees.at(dynData, TRACK);
                        if (trackName != null)
                        {
                            dynData.track = trackManager.AddTrack(trackName);
                        }
                    }
                }

                customBeatmapData.customData.tracks = trackManager.Tracks;

                IEnumerable<dynamic> pointDefinitions = (IEnumerable<dynamic>)Trees.at(customBeatmapData.customData, POINTDEFINITIONS);
                if (pointDefinitions == null)
                {
                    return;
                }

                PointDefinitionManager pointDataManager = new PointDefinitionManager();
                foreach (dynamic pointDefintion in pointDefinitions)
                {
                    string pointName = Trees.at(pointDefintion, NAME);
                    PointDefinition pointData = PointDefinition.DynamicToPointData(Trees.at(pointDefintion, POINTS));
                    pointDataManager.AddPoint(pointName, pointData);
                }

                customBeatmapData.customData.pointDefinitions = pointDataManager.PointData;
            }
        }
    }
}
