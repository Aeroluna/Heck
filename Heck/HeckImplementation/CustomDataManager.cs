using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using IPA.Logging;
using static Heck.HeckController;

namespace Heck
{
    internal class CustomDataManager
    {
        [CustomEventsDeserializer]
        private static Dictionary<CustomEventData, ICustomEventCustomData> DeserializeCustomEvents(
            CustomBeatmapData beatmapData,
            Dictionary<string, PointDefinition> pointDefinitions,
            Dictionary<string, Track> tracks,
            List<CustomEventData> customEventDatas,
            bool v2)
        {
            Dictionary<CustomEventData, ICustomEventCustomData> dictionary = new();
            foreach (CustomEventData customEventData in customEventDatas)
            {
                try
                {
                    switch (customEventData.eventType)
                    {
                        case ANIMATE_TRACK:
                        case ASSIGN_PATH_ANIMATION:
                            dictionary.Add(customEventData, ProcessCoroutineEvent(customEventData, pointDefinitions, tracks, v2));
                            break;

                        case INVOKE_EVENT:
                            if (v2)
                            {
                                break;
                            }

                            IDictionary<string, CustomEventData> eventDefinitions = beatmapData.customData.Get<IDictionary<string, CustomEventData>>(EVENT_DEFINITIONS)
                                                                                    ?? throw new InvalidOperationException("Could not find event definitions in BeatmapData.");
                            string eventName = customEventData.customData.Get<string>(EVENT) ?? throw new InvalidOperationException("Event name was not defined.");
                            dictionary.Add(customEventData, new HeckInvokeEventData(eventDefinitions[eventName]));
                            break;

                        default:
                            continue;
                    }
                }
                catch (Exception e)
                {
                    Log.Logger.LogFailure(e, customEventData);
                }
            }

            return dictionary;
        }

        private static HeckCoroutineEventData ProcessCoroutineEvent(
            CustomEventData customEventData,
            Dictionary<string, PointDefinition> pointDefinitions,
            Dictionary<string, Track> beatmapTracks,
            bool v2)
        {
            HeckCoroutineEventData heckEventData = new();

            Dictionary<string, object?> data = customEventData.customData;

            Functions? easing = data.GetStringToEnum<Functions?>(v2 ? V2_EASING : EASING);
            heckEventData.Easing = easing ?? Functions.easeLinear;

            heckEventData.Duration = data.Get<float?>(v2 ? V2_DURATION : DURATION) ?? 0f;

            IEnumerable<Track> tracks = data.GetTrackArray(beatmapTracks, v2);

            string[] excludedStrings = { V2_TRACK, V2_DURATION, V2_EASING, TRACK, DURATION, EASING };
            IEnumerable<string> propertyKeys = data.Keys.Where(n => excludedStrings.All(m => m != n)).ToList();
            foreach (Track track in tracks)
            {
                IDictionary<string, Property> properties;
                IDictionary<string, List<Property>> propertyAliases;
                switch (customEventData.eventType)
                {
                    case ANIMATE_TRACK:
                        properties = track.Properties;
                        propertyAliases = track.PropertyAliases;
                        break;

                    case ASSIGN_PATH_ANIMATION:
                        properties = track.PathProperties;
                        propertyAliases = track.PathPropertyAliases;
                        break;

                    default:
                        throw new InvalidOperationException("Custom event was not of correct type.");
                }

                foreach (string propertyKey in propertyKeys)
                {
                    void CreateInfo(Property prop)
                    {
                        HeckCoroutineEventData.CoroutineInfo coroutineInfo = new(data.GetPointData(propertyKey, pointDefinitions), prop);
                        heckEventData.CoroutineInfos.Add(coroutineInfo);
                    }

                    if (!v2)
                    {
                        if (properties.TryGetValue(propertyKey, out Property property))
                        {
                            CreateInfo(property);
                            continue;
                        }
                    }
                    else
                    {
                        if (propertyAliases.TryGetValue(propertyKey, out List<Property> aliasedProperties))
                        {
                            aliasedProperties.ForEach(CreateInfo);
                            continue;
                        }
                    }

                    Log.Logger.Log(
                        customEventData.eventType == ASSIGN_PATH_ANIMATION
                            ? $"Could not find path property [{propertyKey}]."
                            : $"Could not find property [{propertyKey}].",
                        Logger.Level.Error);
                }
            }

            return heckEventData;
        }
    }

    internal class HeckCoroutineEventData : ICustomEventCustomData
    {
        internal float Duration { get; set; }

        internal Functions Easing { get; set; }

        internal List<CoroutineInfo> CoroutineInfos { get; } = new();

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

    internal class HeckInvokeEventData : ICustomEventCustomData
    {
        public HeckInvokeEventData(CustomEventData customEventData)
        {
            CustomEventData = customEventData;
        }

        internal CustomEventData CustomEventData { get; }
    }
}
