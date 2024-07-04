using System;
using System.Collections.Generic;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using Heck.Deserialize;
using static Heck.HeckController;

namespace Heck
{
    internal class CustomDataDeserializer : IObjectsDeserializer, ICustomEventsDeserializer
    {
        private readonly CustomBeatmapData _beatmapData;
        private readonly Dictionary<string, List<object>> _pointDefinitions;
        private readonly Dictionary<string, Track> _tracks;
        private readonly float _bpm;

        private CustomDataDeserializer(
            CustomBeatmapData beatmapData,
            Dictionary<string, List<object>> pointDefinitions,
            Dictionary<string, Track> tracks,
            float bpm)
        {
            _beatmapData = beatmapData;
            _pointDefinitions = pointDefinitions;
            _tracks = tracks;
            _bpm = bpm;
        }

        public Dictionary<BeatmapObjectData, IObjectCustomData> DeserializeObjects()
        {
            Dictionary<BeatmapObjectData, IObjectCustomData> dictionary = new();
            foreach (BeatmapObjectData beatmapObjectData in _beatmapData.beatmapObjectDatas)
            {
                CustomData customData = ((ICustomData)beatmapObjectData).customData;
                bool v2;
                if (beatmapObjectData is IVersionable versionable)
                {
                    v2 = versionable.version.IsVersion2();
                }
                else
                {
                    v2 = false;
                }

                dictionary.Add(beatmapObjectData, new HeckObjectData(beatmapObjectData, customData, _tracks, _bpm, v2));
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
                    switch (customEventData.eventType)
                    {
                        case ANIMATE_TRACK:
                        case ASSIGN_PATH_ANIMATION:
                            dictionary.Add(customEventData, new HeckCoroutineEventData(customEventData, _pointDefinitions, _tracks, v2));
                            break;

                        case INVOKE_EVENT:
                            if (v2)
                            {
                                break;
                            }

                            dictionary.Add(customEventData, new HeckInvokeEventData(_beatmapData, customEventData));
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
