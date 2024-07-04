using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using IPA.Utilities;
using JetBrains.Annotations;
using SiraUtil.Logging;
using static Heck.HeckController;

namespace Heck.Deserialize
{
    [UsedImplicitly]
    internal class DeserializerManager
    {
        private readonly SiraLog _log;
        private readonly HashSet<DataDeserializer> _customDataDeserializers = new();

        private DeserializerManager(SiraLog log)
        {
            _log = log;
        }

        internal DataDeserializer Register(string id, Type type)
        {
            DataDeserializer deserializer = new(id, type);
            _customDataDeserializers.Add(deserializer);
            return deserializer;
        }

        internal HashSet<(object? Id, DeserializedData DeserializedData)> EmptyDeserialize()
        {
            HashSet<(object? Id, DeserializedData DeserializedData)> result = new();
            foreach (DataDeserializer dataDeserializer in _customDataDeserializers)
            {
                result.Add((dataDeserializer.Id, new DeserializedData(
                    new Dictionary<CustomEventData, ICustomEventCustomData>(),
                    new Dictionary<BeatmapEventData, IEventCustomData>(),
                    new Dictionary<BeatmapObjectData, IObjectCustomData>())));
            }

            return result;
        }

        internal void DeserializeBeatmapData(
#if LATEST
            BeatmapLevel beatmapLevel,
#else
            IDifficultyBeatmap difficultyBeatmap,
#endif
            CustomBeatmapData customBeatmapData,
            IReadonlyBeatmapData untransformedBeatmapData,
            bool leftHanded,
            out Dictionary<string, Track> beatmapTracks,
            out HashSet<(object? Id, DeserializedData DeserializedData)> deserializedDatas)
        {
            _log.Trace("Deserializing BeatmapData");

            Version version = customBeatmapData.version;
            bool v2 = version.IsVersion2();
            if (v2)
            {
                _log.Trace("BeatmapData is v2, converting...");
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
            Dictionary<string, CustomEventData> eventDefinitions = new();

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
                            eventDefinitions.Add(eventName, new CustomEventData(-1, type, data, version));
                        }
                        else
                        {
                            _log.Error($"Duplicate event defintion name, {eventName} could not be registered");
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

#if LATEST
            float bpm = beatmapLevel.beatsPerMinute;
#else
            float bpm = difficultyBeatmap.level.beatsPerMinute;
#endif

            object[] inputs =
            {
                customBeatmapData,
                trackManager,
                pointDefinitions,
                trackManager.Tracks,
                leftHanded,
                bpm
            };

            DataDeserializer[] deserializers = _customDataDeserializers.Where(n => n.Enabled).ToArray();

            foreach (DataDeserializer deserializer in deserializers)
            {
                deserializer.Create(inputs);
            }

            deserializedDatas = new HashSet<(object? Id, DeserializedData DeserializedData)>(deserializers.Length);
            foreach (DataDeserializer deserializer in deserializers)
            {
                _log.Trace($"Binding [{deserializer.Id}]");

                deserializedDatas.Add((deserializer.Id, deserializer.Deserialize()));
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
                    _log.Error($"Duplicate point defintion name, {pointDataName} could not be registered");
                }
            }
        }
    }
}
