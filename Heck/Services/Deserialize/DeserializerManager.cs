using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
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
            IDifficultyBeatmap difficultyBeatmap,
            CustomBeatmapData customBeatmapData,
            IReadonlyBeatmapData untransformedBeatmapData,
            bool leftHanded,
            out Dictionary<string, Track> beatmapTracks,
            out HashSet<(object? Id, DeserializedData DeserializedData)> deserializedDatas)
        {
            Plugin.Log.Trace("Deserializing BeatmapData");

            bool v2 = customBeatmapData.version2_6_0AndEarlier;
            if (v2)
            {
                Plugin.Log.Trace("BeatmapData is v2, converting...");
            }

            // tracks are built based off the untransformed beatmapdata so modifiers like "no walls" do not prevent track creation
            TrackBuilder trackManager = new();
            foreach (BeatmapObjectData beatmapObjectData in ((CustomBeatmapData)untransformedBeatmapData).beatmapObjectDatas)
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
            Dictionary<string, List<object>> pointDefinitions = new();

            if (v2)
            {
                IEnumerable<CustomData>? pointDefinitionsRaw =
                    customBeatmapData.customData.Get<List<object>>(V2_POINT_DEFINITIONS)?.Cast<CustomData>();
                if (pointDefinitionsRaw != null)
                {
                    foreach (CustomData pointDefintionRaw in pointDefinitionsRaw)
                    {
                        string pointName = pointDefintionRaw.GetRequired<string>(V2_NAME);
                        AddPoint(pointName, pointDefintionRaw.GetRequired<List<object>>(V2_POINTS));
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

                        AddPoint(key, (List<object>)value);
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
                            eventDefinitions.Add(eventName, new CustomEventData(-1, type, data, v2));
                        }
                        else
                        {
                            Plugin.Log.Error($"Duplicate event defintion name, {eventName} could not be registered");
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

            object[] inputs =
            {
                difficultyBeatmap,
                customBeatmapData,
                trackManager,
                pointDefinitions,
                trackManager.Tracks,
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

                Plugin.Log.Trace($"Binding [{deserializer.Id}]");

                deserializedDatas.Add((deserializer.Id, new DeserializedData(customEventCustomDatas, eventCustomDatas, objectCustomDatas)));
            }

            return;

            void AddPoint(string pointDataName, List<object> pointData)
            {
                if (!pointDefinitions.ContainsKey(pointDataName))
                {
                    pointDefinitions.Add(pointDataName, pointData);
                }
                else
                {
                    Plugin.Log.Error($"Duplicate point defintion name, {pointDataName} could not be registered");
                }
            }
        }
    }
}
