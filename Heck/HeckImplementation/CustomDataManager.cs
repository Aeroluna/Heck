using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using static Heck.HeckController;

namespace Heck
{
    internal class CustomDataManager
    {
        [CustomEventsDeserializer]
        private static Dictionary<CustomEventData, ICustomEventCustomData> DeserializeCustomEvents(
            CustomBeatmapData beatmapData,
            Dictionary<string, List<object>> pointDefinitions,
            Dictionary<string, Track> tracks,
            List<CustomEventData> customEventDatas)
        {
            bool v2 = beatmapData.version2_6_0AndEarlier;
            Dictionary<CustomEventData, ICustomEventCustomData> dictionary = new();
            foreach (CustomEventData customEventData in customEventDatas)
            {
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
                    Plugin.Log.LogFailure(e, customEventData);
                }
            }

            return dictionary;
        }
    }
}
