using System;
using System.Collections.Generic;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using Heck.Deserialize;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions
{
    internal class CustomDataDeserializer : IEarlyDeserializer, IObjectsDeserializer, ICustomEventsDeserializer
    {
        private readonly CustomBeatmapData _beatmapData;
        private readonly Dictionary<string, List<object>> _pointDefinitions;
        private readonly Dictionary<string, Track> _tracks;
        private readonly bool _leftHanded;
        private readonly TrackBuilder _trackBuilder;
        private readonly float _bpm;

        private CustomDataDeserializer(
            CustomBeatmapData beatmapData,
            Dictionary<string, List<object>> pointDefinitions,
            Dictionary<string, Track> tracks,
            bool leftHanded,
            TrackBuilder trackBuilder,
            float bpm)
        {
            _beatmapData = beatmapData;
            _pointDefinitions = pointDefinitions;
            _tracks = tracks;
            _leftHanded = leftHanded;
            _trackBuilder = trackBuilder;
            _bpm = bpm;
        }

        public void DeserializeEarly()
        {
            foreach (CustomEventData customEventData in _beatmapData.customEventDatas)
            {
                bool v2 = customEventData.version.IsVersion2();
                try
                {
                    switch (customEventData.eventType)
                    {
                        case ASSIGN_PLAYER_TO_TRACK:
                            _trackBuilder.AddFromCustomData(customEventData.customData, v2);
                            break;

                        case ASSIGN_TRACK_PARENT:
                            _trackBuilder.AddFromCustomData(customEventData.customData, v2 ? V2_PARENT_TRACK : PARENT_TRACK);
                            break;

                        default:
                            continue;
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.DeserializeFailure(e, customEventData, _bpm);
                }
            }
        }

        public Dictionary<BeatmapObjectData, IObjectCustomData> DeserializeObjects()
        {
            Dictionary<BeatmapObjectData, IObjectCustomData> dictionary = new();
            foreach (BeatmapObjectData beatmapObjectData in _beatmapData.beatmapObjectDatas)
            {
                bool v2 = beatmapObjectData is IVersionable versionable && versionable.version.IsVersion2();
                CustomData customData = ((ICustomData)beatmapObjectData).customData;
                switch (beatmapObjectData)
                {
                    case CustomObstacleData customObstacleData:
                        dictionary.Add(beatmapObjectData, new NoodleObstacleData(customObstacleData, customData, _bpm, _pointDefinitions, _tracks, v2, _leftHanded));
                        break;

                    case CustomNoteData customNoteData:
                        dictionary.Add(beatmapObjectData, new NoodleNoteData(customNoteData, customData, _bpm, _pointDefinitions, _tracks, v2, _leftHanded));
                        break;

                    case CustomSliderData customSliderData:
                        dictionary.Add(beatmapObjectData, new NoodleSliderData(customSliderData, customData, _bpm, _pointDefinitions, _tracks, v2, _leftHanded));
                        break;

                    default:
                        dictionary.Add(beatmapObjectData, new NoodleObjectData(beatmapObjectData, customData, _bpm, _pointDefinitions, _tracks, v2, _leftHanded));
                        break;
                }
            }

            return dictionary;
        }

        public Dictionary<CustomEventData, ICustomEventCustomData> DeserializeCustomEvents()
        {
            Dictionary<CustomEventData, ICustomEventCustomData> dictionary = new();
            foreach (CustomEventData customEventData in _beatmapData.customEventDatas)
            {
                bool v2 = customEventData.version.IsVersion2();
                try
                {
                    CustomData data = customEventData.customData;
                    switch (customEventData.eventType)
                    {
                        case ASSIGN_PLAYER_TO_TRACK:
                            dictionary.Add(customEventData, new NoodlePlayerTrackEventData(data, _tracks, v2));
                            break;

                        case ASSIGN_TRACK_PARENT:
                            dictionary.Add(customEventData, new NoodleParentTrackEventData(data, _tracks, v2));
                            break;

                        default:
                            continue;
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.DeserializeFailure(e, customEventData, _bpm);
                }
            }

            return dictionary;
        }
    }
}
