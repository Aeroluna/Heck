using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using Zenject;
using static Heck.HeckController;
using Logger = IPA.Logging.Logger;

namespace Heck
{
    public static class DeserializerManager
    {
        private static readonly HashSet<CustomDataDeserializer> _customDataDeserializers = new();

        public static CustomDataDeserializer RegisterDeserialize<T>(object? id)
        {
            CustomDataDeserializer deserializer = new(id);
            deserializer.Bind<T>();
            _customDataDeserializers.Add(deserializer);
            return deserializer;
        }

        internal static void DeserializeBeatmapDataAndBind(
            DiContainer container,
            bool isMultiplayer,
            IReadonlyBeatmapData beatmapData,
            BeatmapData untransformedBeatmapData)
        {
            if (beatmapData is not CustomBeatmapData customBeatmapData)
            {
                throw new InvalidOperationException($"[{nameof(beatmapData)}] was not type [{nameof(CustomBeatmapData)}].");
            }

            Log.Logger.Log(isMultiplayer ? "Deserializing multiplayer BeatmapData." : "Deserializing singleplayer BeatmapData.", Logger.Level.Trace);

            // tracks are built based off the untransformed beatmapdata so modifiers like "no walls" do not prevent track creation
            TrackBuilder trackManager = new();
            foreach (IReadonlyBeatmapLineData readonlyBeatmapLineData in untransformedBeatmapData.beatmapLinesData)
            {
                BeatmapLineData beatmapLineData = (BeatmapLineData)readonlyBeatmapLineData;
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
                    if (trackNameRaw == null)
                    {
                        continue;
                    }

                    IEnumerable<string> trackNames;
                    if (trackNameRaw is List<object> listTrack)
                    {
                        trackNames = listTrack.Cast<string>();
                    }
                    else
                    {
                        trackNames = new[] { (string)trackNameRaw };
                    }

                    foreach (string trackName in trackNames)
                    {
                        trackManager.AddTrack(trackName);
                    }
                }
            }

            // Point definitions
            Dictionary<string, PointDefinition> pointDefinitions = new();

            void AddPoint(string pointDataName, PointDefinition pointData)
            {
                if (!pointDefinitions.ContainsKey(pointDataName))
                {
                    pointDefinitions.Add(pointDataName, pointData);
                }
                else
                {
                    Log.Logger.Log($"Duplicate point defintion name, {pointDataName} could not be registered!", Logger.Level.Error);
                }
            }

            IEnumerable<Dictionary<string, object?>>? pointDefinitionsRaw =
                customBeatmapData.customData.Get<List<object>>(POINT_DEFINITIONS)?.Cast<Dictionary<string, object?>>();
            if (pointDefinitionsRaw != null)
            {
                foreach (Dictionary<string, object?> pointDefintionRaw in pointDefinitionsRaw)
                {
                    string pointName = pointDefintionRaw.Get<string>(NAME) ?? throw new InvalidOperationException("Failed to retrieve point name.");
                    PointDefinition pointData = PointDefinition.ListToPointDefinition(pointDefintionRaw.Get<List<object>>(POINTS)
                                                                                      ?? throw new InvalidOperationException(
                                                                                          "Failed to retrieve point array."));
                    AddPoint(pointName, pointData);
                }
            }

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
                    Log.Logger.Log($"Duplicate event defintion name, {eventDefinitionName} could not be registered!", Logger.Level.Error);
                }
            }

            IEnumerable<Dictionary<string, object?>>? eventDefinitionsRaw =
                customBeatmapData.customData.Get<List<object>>(EVENT_DEFINITIONS)?.Cast<Dictionary<string, object?>>();
            if (eventDefinitionsRaw != null)
            {
                foreach (Dictionary<string, object?> eventDefinitionRaw in eventDefinitionsRaw)
                {
                    string eventName = eventDefinitionRaw.Get<string>(NAME) ?? throw new InvalidOperationException("Failed to retrieve event name.");
                    string type = eventDefinitionRaw.Get<string>("_type") ?? throw new InvalidOperationException("Failed to retrieve event type.");
                    Dictionary<string, object?> data = eventDefinitionRaw.Get<Dictionary<string, object?>>("_data")
                                                       ?? throw new InvalidOperationException("Failed to retrieve event data.");

                    AddEvent(eventName, new CustomEventData(-1, type, data));
                }
            }

            // new deserialize stuff should make these unnecessary
            ////customBeatmapData.customData["tracks"] = trackManager.Tracks;
            ////customBeatmapData.customData["pointDefinitions"] = pointDefinitions;
            ////customBeatmapData.customData["eventDefinitions"] = eventDefinitions;

            // Currently used by Chroma.GameObjectTrackController
            container.Bind<Dictionary<string, Track>>().FromInstance(trackManager.Tracks).AsSingle();

            IEnumerable<CustomEventData> customEventsData = customBeatmapData.customEventsData;
            customEventsData = customEventsData.Concat(eventDefinitions.Values).ToList();

            object[] inputs =
            {
                isMultiplayer,
                customBeatmapData,
                trackManager,
                pointDefinitions,
                trackManager.Tracks,
                customEventsData.ToList(),
                customBeatmapData.beatmapEventsData.ToList(),
                customBeatmapData.beatmapObjectsData.ToList(),
                container
            };

            CustomDataDeserializer[] deserializers = _customDataDeserializers.Where(n => n.Enabled).ToArray();

            foreach (CustomDataDeserializer deserializer in deserializers)
            {
                deserializer.InjectedInvokeEarly(inputs);
            }

            foreach (CustomDataDeserializer deserializer in deserializers)
            {
                Dictionary<CustomEventData, ICustomEventCustomData> customEventCustomDatas = deserializer.InjectedInvokeCustomEvent(inputs);
                Dictionary<BeatmapEventData, IEventCustomData> eventCustomDatas = deserializer.InjectedInvokeEvent(inputs);
                Dictionary<BeatmapObjectData, IObjectCustomData> objectCustomDatas = deserializer.InjectedInvokeObject(inputs);

                Log.Logger.Log($"Binding [{deserializer.Id}].", Logger.Level.Trace);

                container.Bind<CustomData>()
                    .WithId(deserializer.Id)
                    .FromInstance(new CustomData(customEventCustomDatas, eventCustomDatas, objectCustomDatas));
            }
        }
    }
}
