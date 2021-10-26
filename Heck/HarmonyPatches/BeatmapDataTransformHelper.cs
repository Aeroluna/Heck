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
                TrackBuilder trackManager = new TrackBuilder();
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

                // Point definitions
                IDictionary<string, PointDefinition> pointDefinitions = new Dictionary<string, PointDefinition>();
                void AddPoint(string pointDataName, PointDefinition pointData)
                {
                    if (!pointDefinitions.ContainsKey(pointDataName))
                    {
                        pointDefinitions.Add(pointDataName, pointData);
                    }
                    else
                    {
                        Logger.Log($"Duplicate point defintion name, {pointDataName} could not be registered!", IPA.Logging.Logger.Level.Error);
                    }
                }

                IEnumerable<Dictionary<string, object?>>? pointDefinitionsRaw = customBeatmapData.customData.Get<List<object>>(POINTDEFINITIONS)?.Cast<Dictionary<string, object?>>();
                if (pointDefinitionsRaw != null)
                {
                    foreach (Dictionary<string, object?> pointDefintionRaw in pointDefinitionsRaw)
                    {
                        string pointName = pointDefintionRaw.Get<string>(NAME) ?? throw new InvalidOperationException("Failed to retrieve point name.");
                        PointDefinition pointData = PointDefinition.ListToPointDefinition(pointDefintionRaw.Get<List<object>>(POINTS) ?? throw new InvalidOperationException("Failed to retrieve point array."));
                        AddPoint(pointName, pointData);
                    }
                }

                customBeatmapData.customData["pointDefinitions"] = pointDefinitions;

                // Event definitions
                IDictionary<string, CustomEventData> eventDefinitions = new Dictionary<string, CustomEventData>();
                void AddEvent(string eventDefinitionName, CustomEventData eventDefinition)
                {
                    if (!eventDefinitions.ContainsKey(eventDefinitionName))
                    {
                        eventDefinitions.Add(eventDefinitionName, eventDefinition);
                    }
                    else
                    {
                        Logger.Log($"Duplicate event defintion name, {eventDefinitionName} could not be registered!", IPA.Logging.Logger.Level.Error);
                    }
                }

                IEnumerable<Dictionary<string, object?>>? eventDefinitionsRaw = customBeatmapData.customData.Get<List<object>>(EVENTDEFINITIONS)?.Cast<Dictionary<string, object?>>();
                if (eventDefinitionsRaw != null)
                {
                    foreach (Dictionary<string, object?> eventDefinitionRaw in eventDefinitionsRaw)
                    {
                        string eventName = eventDefinitionRaw.Get<string>(NAME) ?? throw new InvalidOperationException("Failed to retrieve event name.");
                        string type = eventDefinitionRaw.Get<string>("_type") ?? throw new InvalidOperationException("Failed to retrieve event type.");
                        Dictionary<string, object?> data = eventDefinitionRaw.Get<Dictionary<string, object?>>("_data") ?? throw new InvalidOperationException("Failed to retrieve event data.");

                        AddEvent(eventName, new CustomEventData(-1, type, data));
                    }
                }

                customBeatmapData.customData["eventDefinitions"] = eventDefinitions;

                StackTrace stackTrace = new StackTrace();
                bool isMultiplayer = stackTrace.GetFrame(2).GetMethod().Name.Contains("MultiplayerConnectedPlayerInstaller");

                customBeatmapData.customData["isMultiplayer"] = isMultiplayer;
                CustomDataDeserializer.InvokeDeserializeBeatmapData(isMultiplayer, customBeatmapData, trackManager);

                if (isMultiplayer)
                {
                    Logger.Log("Deserializing multiplayer BeatmapData.", IPA.Logging.Logger.Level.Trace);
                }
                else
                {
                    Logger.Log("Deserializing local player BeatmapData.", IPA.Logging.Logger.Level.Trace);
                }
            }
        }
    }
}
