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
        private static Dictionary<CustomEventData, HeckEventData> _heckEventDatas = new Dictionary<CustomEventData, HeckEventData>();

        internal static HeckEventData? TryGetEventData(CustomEventData customEventData)
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

                    _heckEventDatas.Add(customEventData, heckEventData);
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not create HeckEventData for event {customEventData.type} at {customEventData.time}", IPA.Logging.Logger.Level.Error);
                    Logger.Log(e, IPA.Logging.Logger.Level.Error);
                }
            }
        }

        private static HeckEventData ProcessCoroutineEvent(CustomEventData customEventData, IReadonlyBeatmapData beatmapData)
        {
            HeckEventData heckEventData = new HeckEventData();

            string? easingString = customEventData.data.Get<string>(EASING);
            heckEventData.Easing = Functions.easeLinear;
            if (easingString != null)
            {
                heckEventData.Easing = (Functions)Enum.Parse(typeof(Functions), easingString);
            }

            heckEventData.Duration = customEventData.data.Get<float?>(DURATION) ?? 0f;

            Track track = GetTrack(customEventData.data, beatmapData) ?? throw new InvalidOperationException("Track was not defined.");

            List<string> excludedStrings = new List<string> { TRACK, DURATION, EASING };
            IDictionary<string, Property> properties;
            switch (customEventData.type)
            {
                case ANIMATETRACK:
                    properties = track.Properties;
                    break;

                case ASSIGNPATHANIMATION:
                    properties = track.PathProperties;
                    break;

                default:
                    throw new InvalidOperationException("Custom event was not of correct type.");
            }

            foreach (KeyValuePair<string, object?> valuePair in customEventData.data)
            {
                if (!excludedStrings.Any(n => n == valuePair.Key))
                {
                    if (!properties.TryGetValue(valuePair.Key, out Property property))
                    {
                        Logger.Log($"Could not find property {valuePair.Key}!", IPA.Logging.Logger.Level.Error);
                        continue;
                    }

                    Dictionary<string, PointDefinition> pointDefinitions = (Dictionary<string, PointDefinition>)(((CustomBeatmapData)beatmapData).customData["pointDefinitions"] ?? throw new InvalidOperationException("Failed to retrieve point definitions."));
                    TryGetPointData(customEventData.data, valuePair.Key, out PointDefinition? pointData, pointDefinitions);

                    HeckEventData.CoroutineInfo coroutineInfo = new HeckEventData.CoroutineInfo(pointData, property);

                    heckEventData.CoroutineInfos.Add(coroutineInfo);
                }
            }

            return heckEventData;
        }
    }

    internal record HeckEventData
    {
        internal float Duration { get; set; }

        internal Functions Easing { get; set; }

        internal List<CoroutineInfo> CoroutineInfos { get; } = new List<CoroutineInfo>();

        internal record CoroutineInfo
        {
            internal CoroutineInfo(PointDefinition? pointDefinition, Property property)
            {
                PointDefinition = pointDefinition;
                Property = property;
            }

            internal PointDefinition? PointDefinition { get; }

            internal Property Property { get; }
        }
    }
}
