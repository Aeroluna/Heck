using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using static Heck.HeckController;

namespace Heck
{
    internal class CustomDataDeserializer : IObjectsDeserializer, ICustomEventsDeserializer
    {
        private readonly CustomBeatmapData _beatmapData;
        private readonly IDifficultyBeatmap _difficultyBeatmap;
        private readonly Dictionary<string, List<object>> _pointDefinitions;
        private readonly Dictionary<string, Track> _tracks;

        private CustomDataDeserializer(
            CustomBeatmapData beatmapData,
            IDifficultyBeatmap difficultyBeatmap,
            Dictionary<string, List<object>> pointDefinitions,
            Dictionary<string, Track> tracks)
        {
            _beatmapData = beatmapData;
            _difficultyBeatmap = difficultyBeatmap;
            _pointDefinitions = pointDefinitions;
            _tracks = tracks;
        }

        public Dictionary<BeatmapObjectData, IObjectCustomData> DeserializeObjects()
        {
            Dictionary<BeatmapObjectData, IObjectCustomData> dictionary = new();
            foreach (BeatmapObjectData beatmapObjectData in _beatmapData.beatmapObjectDatas)
            {
                CustomData customData = ((ICustomData)beatmapObjectData).customData;
                bool v2 = beatmapObjectData is IVersionable { version2_6_0AndEarlier: true };
                dictionary.Add(beatmapObjectData, new HeckObjectData(beatmapObjectData, customData, _difficultyBeatmap, _tracks, v2));
            }

            return dictionary;
        }

        public Dictionary<CustomEventData, ICustomEventCustomData> DeserializeCustomEvents()
        {
            Dictionary<CustomEventData, ICustomEventCustomData> dictionary = new();
            foreach (CustomEventData customEventData in _beatmapData.customEventDatas)
            {
                bool v2 = customEventData.version2_6_0AndEarlier;
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
                    Plugin.Log.DeserializeFailure(e, customEventData, _difficultyBeatmap);
                }
            }

            return dictionary;
        }
    }
}
