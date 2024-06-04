using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using static Heck.HeckController;

namespace Heck
{
    internal static class CustomDataManager
    {
        [ObjectsDeserializer]
        private static Dictionary<BeatmapObjectData, IObjectCustomData> DeserializeObjects(
            CustomBeatmapData beatmapData,
            IDifficultyBeatmap difficultyBeatmap,
            Dictionary<string, Track> beatmapTracks)
        {
            Dictionary<BeatmapObjectData, IObjectCustomData> dictionary = new();
            foreach (BeatmapObjectData beatmapObjectData in beatmapData.beatmapObjectDatas)
            {
                CustomData customData = ((ICustomData)beatmapObjectData).customData;
                bool v2 = beatmapObjectData is IVersionable { version2_6_0AndEarlier: true };
                dictionary.Add(beatmapObjectData, new HeckObjectData(beatmapObjectData, customData, difficultyBeatmap, beatmapTracks, v2));
            }

            return dictionary;
        }

        [CustomEventsDeserializer]
        private static Dictionary<CustomEventData, ICustomEventCustomData> DeserializeCustomEvents(
            CustomBeatmapData beatmapData,
            IDifficultyBeatmap difficultyBeatmap,
            Dictionary<string, List<object>> pointDefinitions,
            Dictionary<string, Track> tracks)
        {
            Dictionary<CustomEventData, ICustomEventCustomData> dictionary = new();
            foreach (CustomEventData customEventData in beatmapData.customEventDatas)
            {
                bool v2 = customEventData.version2_6_0AndEarlier;
                try
                {
                    switch (customEventData.eventType)
                    {
                        case ANIMATE_TRACK:
                        case ASSIGN_PATH_ANIMATION:
                            dictionary.Add(customEventData, new HeckCoroutineEventData(customEventData, pointDefinitions, tracks, v2));
                            break;

                        case INVOKE_EVENT:
                            if (v2)
                            {
                                break;
                            }

                            dictionary.Add(customEventData, new HeckInvokeEventData(beatmapData, customEventData));
                            break;

                        default:
                            continue;
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.DeserializeFailure(e, customEventData, difficultyBeatmap);
                }
            }

            return dictionary;
        }
    }
}
