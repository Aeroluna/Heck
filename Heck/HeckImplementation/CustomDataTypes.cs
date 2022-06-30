using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using IPA.Logging;
using static Heck.HeckController;

namespace Heck
{
    internal class HeckCoroutineEventData : ICustomEventCustomData
    {
        internal HeckCoroutineEventData(
            CustomEventData customEventData,
            Dictionary<string, PointDefinition> pointDefinitions,
            Dictionary<string, Track> beatmapTracks,
            bool v2)
        {
            CustomData data = customEventData.customData;

            IEnumerable<Track> tracks = data.GetTrackArray(beatmapTracks, v2);

            string[] excludedStrings = { V2_TRACK, V2_DURATION, V2_EASING, TRACK, DURATION, EASING, REPEAT };
            IEnumerable<string> propertyKeys = data.Keys.Where(n => excludedStrings.All(m => m != n)).ToList();
            List<CoroutineInfo> coroutineInfos = new();
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
                        CoroutineInfo coroutineInfo = new(data.GetPointData(propertyKey, pointDefinitions), prop);
                        coroutineInfos.Add(coroutineInfo);
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

            Duration = data.Get<float?>(v2 ? V2_DURATION : DURATION) ?? 0f;
            Easing = data.GetStringToEnum<Functions?>(v2 ? V2_EASING : EASING) ?? Functions.easeLinear;
            CoroutineInfos = coroutineInfos;

            if (!v2)
            {
                Repeat = data.Get<int?>(REPEAT) ?? 0;
            }
        }

        internal float Duration { get; }

        internal Functions Easing { get; }

        internal int Repeat { get; }

        internal List<CoroutineInfo> CoroutineInfos { get; }

        internal readonly struct CoroutineInfo
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
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        internal HeckInvokeEventData(CustomBeatmapData beatmapData, CustomEventData customEventData)
        {
            IDictionary<string, CustomEventData> eventDefinitions = beatmapData.customData.GetRequired<IDictionary<string, CustomEventData>>(EVENT_DEFINITIONS);
            string eventName = customEventData.customData.GetRequired<string>(EVENT);
            CustomEventData = eventDefinitions[eventName];
        }

        internal CustomEventData CustomEventData { get; }
    }
}
