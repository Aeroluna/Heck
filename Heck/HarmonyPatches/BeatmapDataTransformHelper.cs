namespace Heck.HarmonyPatches
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using Heck.Animation;
    using static Heck.Plugin;

    [HarmonyPatch(typeof(BeatmapDataTransformHelper))]
    [HarmonyPatch("CreateTransformedBeatmapData")]
    internal static class BeatmapDataTransformHelperCreateTransformedBeatmapData
    {
        [HarmonyPriority(Priority.High)]
        private static void Postfix(IReadonlyBeatmapData __result)
        {
            if (__result is CustomBeatmapData customBeatmapData)
            {
                TrackBuilder trackManager = new TrackBuilder(customBeatmapData);
                foreach (BeatmapLineData beatmapLineData in customBeatmapData.beatmapLinesData)
                {
                    foreach (BeatmapObjectData beatmapObjectData in beatmapLineData.beatmapObjectsData)
                    {
                        Dictionary<string, object?> dynData;
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
                        object? trackNameRaw = dynData.Get<object>(TRACK);
                        if (trackNameRaw != null)
                        {
                            IEnumerable<string> trackNames;
                            if (trackNameRaw is List<object> listTrack)
                            {
                                trackNames = listTrack.Cast<string>();
                            }
                            else
                            {
                                trackNames = new string[] { (string)trackNameRaw };
                            }

                            foreach (string trackName in trackNames)
                            {
                                trackManager.AddTrack(trackName);
                            }
                        }
                    }
                }

                customBeatmapData.customData["tracks"] = trackManager.Tracks;

                PointDefinitionBuilder pointDataManager = new PointDefinitionBuilder();
                IEnumerable<Dictionary<string, object?>>? pointDefinitions = customBeatmapData.customData.Get<List<object>>(POINTDEFINITIONS)?.Cast<Dictionary<string, object?>>();
                if (pointDefinitions != null)
                {
                    foreach (Dictionary<string, object?> pointDefintion in pointDefinitions)
                    {
                        string pointName = pointDefintion.Get<string>(NAME) ?? throw new InvalidOperationException("Failed to retrieve point name.");
                        PointDefinition pointData = PointDefinition.ListToPointDefinition(pointDefintion.Get<List<object>>(POINTS) ?? throw new InvalidOperationException("Failed to retrieve point array."));
                        pointDataManager.AddPoint(pointName, pointData);
                    }
                }

                customBeatmapData.customData["pointDefinitions"] = pointDataManager.PointData;

                // EVENT DATA STUFF HERE
                // Skip if calling class is MultiplayerConnectPlayerInstaller
                StackTrace stackTrace = new StackTrace();
                if (!stackTrace.GetFrame(2).GetMethod().Name.Contains("MultiplayerConnectedPlayerInstaller"))
                {
                    HeckEventDataManager.DeserializeBeatmapData(customBeatmapData);
                }
            }
        }
    }
}
