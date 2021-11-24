using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using static Heck.Animation.AnimationHelper;
using static Heck.HeckController;

namespace Heck
{
    internal static class HeckCustomDataManager
    {
        private static Dictionary<CustomEventData, ICustomEventCustomData> _heckEventDatas = new();

        internal static T? TryGetEventData<T>(CustomEventData customEventData)
            where T : ICustomEventCustomData
        {
            return _heckEventDatas.TryGetCustomData<T>(customEventData);
        }

        internal static void DeserializeCustomEvents(CustomDataDeserializer.DeserializeBeatmapEventArgs eventArgs)
        {
            if (eventArgs.IsMultiplayer)
            {
                return;
            }

            CustomBeatmapData beatmapData = eventArgs.BeatmapData;
            _heckEventDatas = new Dictionary<CustomEventData, ICustomEventCustomData>();
            Dictionary<string, PointDefinition> pointDefinitions = beatmapData.GetBeatmapPointDefinitions();
            Dictionary<string, Track> beatmapTracks = beatmapData.GetBeatmapTracks();
            foreach (CustomEventData customEventData in eventArgs.CustomEventDatas)
            {
                try
                {
                    ICustomEventCustomData heckEventData;

                    switch (customEventData.type)
                    {
                        case ANIMATE_TRACK:
                        case ASSIGN_PATH_ANIMATION:
                            heckEventData = ProcessCoroutineEvent(customEventData, pointDefinitions, beatmapTracks);
                            break;

                        case INVOKE_EVENT:
                            IDictionary<string, CustomEventData> eventDefinitions = beatmapData.customData.Get<IDictionary<string, CustomEventData>>("eventDefinitions") ?? throw new InvalidOperationException("Could not find event definitions in BeatmapData.");
                            string eventName = customEventData.data.Get<string>(EVENT) ?? throw new InvalidOperationException("Event name was not defined.");
                            heckEventData = new HeckInvokeEventData(eventDefinitions[eventName]);
                            break;

                        default:
                            continue;
                    }

                    _heckEventDatas.Add(customEventData, heckEventData);
                }
                catch (Exception e)
                {
                    CustomDataDeserializer.LogFailure(Log.Logger, e, customEventData);
                }
            }
        }

        private static HeckCoroutineEventData ProcessCoroutineEvent(CustomEventData customEventData, Dictionary<string, PointDefinition> pointDefinitions, Dictionary<string, Track> beatmapTracks)
        {
            HeckCoroutineEventData heckEventData = new();

            Dictionary<string, object?> data = customEventData.data;

            Functions? easing = data.GetStringToEnum<Functions?>(EASING);
            heckEventData.Easing = easing ?? Functions.easeLinear;

            heckEventData.Duration = data.Get<float?>(DURATION) ?? 0f;

            IEnumerable<Track> tracks = GetTrackArray(data, beatmapTracks) ?? throw new InvalidOperationException("Track was not defined.");

            string[] excludedStrings = { TRACK, DURATION, EASING };
            IEnumerable<string> propertyKeys = data.Keys.Where(n => excludedStrings.All(m => m != n)).ToList();
            foreach (Track track in tracks)
            {
                IDictionary<string, Property> properties = customEventData.type switch
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

                    HeckCoroutineEventData.CoroutineInfo coroutineInfo = new(TryGetPointData(data, propertyKey, pointDefinitions), property);

                    heckEventData.CoroutineInfos.Add(coroutineInfo);
                }
            }

            return heckEventData;
        }
    }

    internal record HeckCoroutineEventData : ICustomEventCustomData
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

    internal record HeckInvokeEventData : ICustomEventCustomData
    {
        public HeckInvokeEventData(CustomEventData customEventData)
        {
            CustomEventData = customEventData;
        }

        internal CustomEventData CustomEventData { get; }
    }
}
