using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
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
            Dictionary<string, PointDefinition> pointDefinitions,
            Dictionary<string, Track> tracks,
            List<CustomEventData> customEventDatas)
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
                            dictionary.Add(customEventData, ProcessCoroutineEvent(customEventData, pointDefinitions, tracks));
                            break;

                        case INVOKE_EVENT:
                            IDictionary<string, CustomEventData> eventDefinitions = beatmapData.customData.Get<IDictionary<string, CustomEventData>>("eventDefinitions")
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

        private static HeckCoroutineEventData ProcessCoroutineEvent(CustomEventData customEventData, Dictionary<string, PointDefinition> pointDefinitions, Dictionary<string, Track> beatmapTracks)
        {
            HeckCoroutineEventData heckEventData = new();

            Dictionary<string, object?> data = customEventData.customData;

            Functions? easing = data.GetStringToEnum<Functions?>(EASING);
            heckEventData.Easing = easing ?? Functions.easeLinear;

            heckEventData.Duration = data.Get<float?>(DURATION) ?? 0f;

            IEnumerable<Track> tracks = data.GetTrackArray(beatmapTracks) ?? throw new InvalidOperationException("Track was not defined.");

            string[] excludedStrings = { TRACK, DURATION, EASING };
            IEnumerable<string> propertyKeys = data.Keys.Where(n => excludedStrings.All(m => m != n)).ToList();
            foreach (Track track in tracks)
            {
                IDictionary<string, Property> properties = customEventData.eventType switch
                {
                    ANIMATE_TRACK => track.Properties,
                    ASSIGN_PATH_ANIMATION => track.PathProperties,
                    _ => throw new InvalidOperationException("Custom event was not of correct type.")
                };

                foreach (string propertyKey in propertyKeys)
                {
                    if (!properties.TryGetValue(propertyKey, out Property property))
                    {
                        Log.Logger.Log($"Could not find property {propertyKey}!", IPA.Logging.Logger.Level.Error);
                        continue;
                    }

                    HeckCoroutineEventData.CoroutineInfo coroutineInfo = new(data.GetPointData(propertyKey, pointDefinitions), property);

                    heckEventData.CoroutineInfos.Add(coroutineInfo);
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
