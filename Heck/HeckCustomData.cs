namespace Heck
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using Heck.Animation;
    using static Heck.Animation.AnimationHelper;
    using static Heck.Plugin;

    internal static class HeckCustomDataManager
    {
        private static Dictionary<CustomEventData, ICustomEventCustomData> _heckEventDatas = new Dictionary<CustomEventData, ICustomEventCustomData>();

        internal static HeckEventData? TryGetEventData(CustomEventData customEventData)
        {
            return _heckEventDatas.TryGetCustomData<HeckEventData>(customEventData);
        }

        internal static void DeserializeCustomEvents(bool isMultiplayer, IEnumerable<CustomEventData> customEventsData, CustomBeatmapData beatmapData)
        {
            if (isMultiplayer)
            {
                return;
            }

            _heckEventDatas = new Dictionary<CustomEventData, ICustomEventCustomData>();
            Dictionary<string, PointDefinition> pointDefinitions = beatmapData.GetBeatmapPointDefinitions();
            Dictionary<string, Track> beatmapTracks = beatmapData.GetBeatmapTracks();
            foreach (CustomEventData customEventData in customEventsData)
            {
                try
                {
                    HeckEventData heckEventData;

                    switch (customEventData.type)
                    {
                        case ANIMATETRACK:
                        case ASSIGNPATHANIMATION:
                            heckEventData = ProcessCoroutineEvent(customEventData, pointDefinitions, beatmapTracks);
                            break;

                        default:
                            continue;
                    }

                    _heckEventDatas.Add(customEventData, heckEventData);
                }
                catch (Exception e)
                {
                    CustomDataDeserializer.LogFailure(Logger, e, customEventData);
                }
            }
        }

        private static HeckEventData ProcessCoroutineEvent(CustomEventData customEventData, Dictionary<string, PointDefinition> pointDefinitions, Dictionary<string, Track> beatmapTracks)
        {
            HeckEventData heckEventData = new HeckEventData();

            Dictionary<string, object?> data = customEventData.data;
            string? easingString = data.Get<string>(EASING);
            heckEventData.Easing = Functions.easeLinear;
            if (easingString != null)
            {
                heckEventData.Easing = (Functions)Enum.Parse(typeof(Functions), easingString);
            }

            heckEventData.Duration = data.Get<float?>(DURATION) ?? 0f;

            Track track = GetTrack(data, beatmapTracks) ?? throw new InvalidOperationException("Track was not defined.");

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

            string[] excludedStrings = new string[] { TRACK, DURATION, EASING };
            foreach (KeyValuePair<string, object?> valuePair in data)
            {
                if (!excludedStrings.Any(n => n == valuePair.Key))
                {
                    if (!properties.TryGetValue(valuePair.Key, out Property property))
                    {
                        Logger.Log($"Could not find property {valuePair.Key}!", IPA.Logging.Logger.Level.Error);
                        continue;
                    }

                    HeckEventData.CoroutineInfo coroutineInfo = new HeckEventData.CoroutineInfo(TryGetPointData(data, valuePair.Key, pointDefinitions), property);

                    heckEventData.CoroutineInfos.Add(coroutineInfo);
                }
            }

            return heckEventData;
        }
    }

    internal record HeckEventData : ICustomEventCustomData
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
