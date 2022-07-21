using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using IPA.Logging;
using IPA.Utilities;
using static Heck.HeckController;

namespace Heck
{
    public static class DeserializerManager
    {
        private static readonly HashSet<DataDeserializer> _customDataDeserializers = new();

        public static DataDeserializer Register<T>(object? id)
        {
            DataDeserializer deserializer = new(id, typeof(T));
            _customDataDeserializers.Add(deserializer);
            return deserializer;
        }

        internal static void DeserializeBeatmapData(
            CustomBeatmapData customBeatmapData,
            IReadonlyBeatmapData untransformedBeatmapData,
            bool leftHanded,
            out Dictionary<string, Track> beatmapTracks,
            out HashSet<(object? Id, DeserializedData DeserializedData)> deserializedDatas)
        {
            Log.Logger.Log("Deserializing BeatmapData.", Logger.Level.Trace);

            bool v2 = customBeatmapData.version2_6_0AndEarlier;
            if (v2)
            {
                Log.Logger.Log("BeatmapData is v2, converting...", Logger.Level.Trace);
            }

            // tracks are built based off the untransformed beatmapdata so modifiers like "no walls" do not prevent track creation
            TrackBuilder trackManager = new(v2);
            IReadOnlyList<BeatmapObjectData> untransformedObjectDatas = untransformedBeatmapData.GetBeatmapDataItems<NoteData>()
                .Cast<BeatmapObjectData>()
                .Concat(untransformedBeatmapData.GetBeatmapDataItems<ObstacleData>())
                .Concat(untransformedBeatmapData.GetBeatmapDataItems<SliderData>())
                .ToArray();
            foreach (BeatmapObjectData beatmapObjectData in untransformedObjectDatas)
            {
                CustomData customData = ((ICustomData)beatmapObjectData).customData;

                // for epic tracks thing
                object? trackNameRaw = customData.Get<object>(v2 ? V2_TRACK : TRACK);
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

            if (v2)
            {
                IEnumerable<CustomData>? pointDefinitionsRaw =
                    customBeatmapData.customData.Get<List<object>>(V2_POINT_DEFINITIONS)?.Cast<CustomData>();
                if (pointDefinitionsRaw != null)
                {
                    foreach (CustomData pointDefintionRaw in pointDefinitionsRaw)
                    {
                        string pointName = pointDefintionRaw.GetRequired<string>(V2_NAME);
                        PointDefinition pointData = PointDefinition.ListToPointDefinition(
                            pointDefintionRaw.GetRequired<List<object>>(V2_POINTS));
                        AddPoint(pointName, pointData);
                    }
                }
            }
            else
            {
                CustomData? pointDefinitionsRaw = customBeatmapData.customData.Get<CustomData>(POINT_DEFINITIONS);
                if (pointDefinitionsRaw != null)
                {
                    foreach ((string key, object? value) in pointDefinitionsRaw)
                    {
                        if (value == null)
                        {
                            throw new InvalidOperationException($"[{key}] was null.");
                        }

                        PointDefinition pointData = PointDefinition.ListToPointDefinition((List<object>)value);
                        AddPoint(key, pointData);
                    }
                }
            }

            // Event definitions
            IDictionary<string, CustomEventData> eventDefinitions = new Dictionary<string, CustomEventData>();

            if (!v2)
            {
                IEnumerable<CustomData>? eventDefinitionsRaw =
                    customBeatmapData.customData.Get<List<object>>(EVENT_DEFINITIONS)?.Cast<CustomData>();
                if (eventDefinitionsRaw != null)
                {
                    foreach (CustomData eventDefinitionRaw in eventDefinitionsRaw)
                    {
                        string eventName = eventDefinitionRaw.GetRequired<string>(NAME);
                        string type = eventDefinitionRaw.GetRequired<string>(TYPE);
                        CustomData data = eventDefinitionRaw.GetRequired<CustomData>("data");

                        if (!eventDefinitions.ContainsKey(eventName))
                        {
                            eventDefinitions.Add(eventName, new CustomEventData(-1, type, data));
                        }
                        else
                        {
                            Log.Logger.Log($"Duplicate event defintion name, {eventName} could not be registered!", Logger.Level.Error);
                        }
                    }
                }
            }

            // new deserialize stuff should make these unnecessary
            ////customBeatmapData.customData["tracks"] = trackManager.Tracks;
            ////customBeatmapData.customData["pointDefinitions"] = pointDefinitions;
            ////customBeatmapData.customData["eventDefinitions"] = eventDefinitions;

            // Currently used by Chroma.GameObjectTrackController
            beatmapTracks = trackManager.Tracks;

            IReadOnlyList<CustomEventData> customEventsData = customBeatmapData
                .GetBeatmapDataItems<CustomEventData>()
                .Concat(eventDefinitions.Values)
                .ToArray();

            IReadOnlyList<BeatmapEventData> beatmapEventDatas = customBeatmapData.GetBeatmapDataItems<BasicBeatmapEventData>()
                .Cast<BeatmapEventData>()
                .Concat(customBeatmapData.GetBeatmapDataItems<SpawnRotationBeatmapEventData>())
                .Concat(customBeatmapData.GetBeatmapDataItems<BPMChangeBeatmapEventData>())
                .Concat(customBeatmapData.GetBeatmapDataItems<LightColorBeatmapEventData>())
                .Concat(customBeatmapData.GetBeatmapDataItems<LightRotationBeatmapEventData>())
                .Concat(customBeatmapData.GetBeatmapDataItems<ColorBoostBeatmapEventData>())
                .ToArray();

            IReadOnlyList<BeatmapObjectData> objectDatas = customBeatmapData.GetBeatmapDataItems<NoteData>()
                .Cast<BeatmapObjectData>()
                .Concat(customBeatmapData.GetBeatmapDataItems<ObstacleData>())
                .Concat(customBeatmapData.GetBeatmapDataItems<SliderData>())
                .ToArray();

            object[] inputs =
            {
                customBeatmapData,
                trackManager,
                pointDefinitions,
                trackManager.Tracks,
                customEventsData.ToList(),
                beatmapEventDatas,
                objectDatas,
                leftHanded
            };

            DataDeserializer[] deserializers = _customDataDeserializers.Where(n => n.Enabled).ToArray();

            foreach (DataDeserializer deserializer in deserializers)
            {
                deserializer.InjectedInvokeEarly(inputs);
            }

            deserializedDatas = new HashSet<(object? Id, DeserializedData DeserializedData)>(deserializers.Length);
            foreach (DataDeserializer deserializer in deserializers)
            {
                Dictionary<CustomEventData, ICustomEventCustomData> customEventCustomDatas = deserializer.InjectedInvokeCustomEvent(inputs);
                Dictionary<BeatmapEventData, IEventCustomData> eventCustomDatas = deserializer.InjectedInvokeEvent(inputs);
                Dictionary<BeatmapObjectData, IObjectCustomData> objectCustomDatas = deserializer.InjectedInvokeObject(inputs);

                Log.Logger.Log($"Binding [{deserializer.Id}].", Logger.Level.Trace);

                deserializedDatas.Add((deserializer.Id, new DeserializedData(customEventCustomDatas, eventCustomDatas, objectCustomDatas)));
            }
        }
    }
}
