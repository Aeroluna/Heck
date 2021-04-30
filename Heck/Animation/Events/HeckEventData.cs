namespace Heck.Animation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using static Heck.Animation.AnimationHelper;
    using static Heck.Plugin;

    internal static class HeckEventDataManager
    {
        private static Dictionary<CustomEventData, HeckEventData> _heckEventDatas;

        internal static HeckEventData TryGetEventData(CustomEventData customEventData)
        {
            if (_heckEventDatas.TryGetValue(customEventData, out HeckEventData noodleEventData))
            {
                return noodleEventData;
            }

            return null;
        }

        internal static void DeserializeBeatmapData(IReadonlyBeatmapData beatmapData)
        {
            _heckEventDatas = new Dictionary<CustomEventData, HeckEventData>();
            foreach (CustomEventData customEventData in ((CustomBeatmapData)beatmapData).customEventsData)
            {
                try
                {
                    HeckEventData heckEventData;

                    switch (customEventData.type)
                    {
                        case ANIMATETRACK:
                        case ASSIGNPATHANIMATION:
                            heckEventData = ProcessCoroutineEvent(customEventData, beatmapData);
                            break;

                        default:
                            continue;
                    }

                    if (heckEventData != null)
                    {
                        _heckEventDatas.Add(customEventData, heckEventData);
                    }
                }
                catch (Exception e)
                {
                    Plugin.Logger.Log($"Could not create HeckEventData for event {customEventData.type} at {customEventData.time}", IPA.Logging.Logger.Level.Error);
                    Plugin.Logger.Log(e, IPA.Logging.Logger.Level.Error);
                }
            }
        }

        private static HeckEventData ProcessCoroutineEvent(CustomEventData customEventData, IReadonlyBeatmapData beatmapData)
        {
            HeckEventData heckEventData = new HeckEventData();

            string easingString = (string)Trees.at(customEventData.data, EASING);
            heckEventData.Easing = Functions.easeLinear;
            if (easingString != null)
            {
                heckEventData.Easing = (Functions)Enum.Parse(typeof(Functions), easingString);
            }

            heckEventData.Duration = (float?)Trees.at(customEventData.data, DURATION) ?? 0f;

            Track track = GetTrackPreload(customEventData.data, beatmapData);
            if (track == null)
            {
                return null;
            }

            EventType eventType;
            switch (customEventData.type)
            {
                case ANIMATETRACK:
                    eventType = EventType.AnimateTrack;
                    break;

                case ASSIGNPATHANIMATION:
                    eventType = EventType.AssignPathAnimation;
                    break;

                default:
                    return null;
            }

            List<string> excludedStrings = new List<string> { TRACK, DURATION, EASING };
            IDictionary<string, object> eventData = new Dictionary<string, object>(customEventData.data as IDictionary<string, object>); // Shallow copy
            IDictionary<string, Property> properties = null;
            switch (eventType)
            {
                case EventType.AnimateTrack:
                    properties = track.Properties;
                    break;

                case EventType.AssignPathAnimation:
                    properties = track.PathProperties;
                    break;
            }

            foreach (KeyValuePair<string, object> valuePair in eventData)
            {
                if (!excludedStrings.Any(n => n == valuePair.Key))
                {
                    if (!properties.TryGetValue(valuePair.Key, out Property property))
                    {
                        Plugin.Logger.Log($"Could not find property {valuePair.Key}!", IPA.Logging.Logger.Level.Error);
                        continue;
                    }

                    Dictionary<string, PointDefinition> pointDefinitions = Trees.at(((CustomBeatmapData)beatmapData).customData, "pointDefinitions");
                    TryGetPointData(customEventData.data, valuePair.Key, out PointDefinition pointData, pointDefinitions);

                    HeckEventData.CoroutineInfo coroutineInfo = new HeckEventData.CoroutineInfo()
                    {
                        PointDefinition = pointData,
                        Property = property,
                    };

                    heckEventData.CoroutineInfos.Add(coroutineInfo);
                }
            }

            return heckEventData;
        }
    }

    internal class HeckEventData
    {
        internal float Duration { get; set; }

        internal Functions Easing { get; set; }

        internal List<CoroutineInfo> CoroutineInfos { get; set; } = new List<CoroutineInfo>();

        internal class CoroutineInfo
        {
            internal PointDefinition PointDefinition { get; set; }

            internal Property Property { get; set; }
        }
    }
}
