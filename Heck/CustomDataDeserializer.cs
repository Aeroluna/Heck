namespace Heck
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using Heck.Animation;

    public interface ICustomEventCustomData
    {
    }

    public interface IBeatmapEventDataCustomData
    {
    }

    public interface IBeatmapObjectDataCustomData
    {
    }

    public static class CustomDataDeserializer
    {
        public static event Action<DeserializeBeatmapEventArgs>? DeserializeBeatmapData;

        public static event Action<DeserializeBeatmapEventArgs>? BuildTracks;

        public static T? TryGetCustomData<T>(this IDictionary<CustomEventData, ICustomEventCustomData> dictionary, CustomEventData customEventData)
            where T : ICustomEventCustomData?
        {
            return dictionary.TryGetCustomData<CustomEventData, ICustomEventCustomData, T>(customEventData);
        }

        public static T? TryGetCustomData<T>(this IDictionary<BeatmapEventData, IBeatmapEventDataCustomData> dictionary, BeatmapEventData beatmapEventData)
            where T : IBeatmapEventDataCustomData?
        {
            return dictionary.TryGetCustomData<BeatmapEventData, IBeatmapEventDataCustomData, T>(beatmapEventData);
        }

        public static T? TryGetCustomData<T>(this IDictionary<BeatmapObjectData, IBeatmapObjectDataCustomData> dictionary, BeatmapObjectData beatmapObjectData)
            where T : IBeatmapObjectDataCustomData?
        {
            return dictionary.TryGetCustomData<BeatmapObjectData, IBeatmapObjectDataCustomData, T>(beatmapObjectData);
        }

        public static void LogFailure(HeckLogger logger, Exception e, CustomEventData customEventData)
        {
            logger.Log($"Could not parse custom data for custom event [{customEventData.type}] at [{customEventData.time}].", IPA.Logging.Logger.Level.Error);
            logger.Log(e, IPA.Logging.Logger.Level.Error);
        }

        public static void LogFailure(HeckLogger logger, Exception e, BeatmapEventData beatmapEventData)
        {
            logger.Log($"Could not parse custom data for event [{beatmapEventData.type}] at [{beatmapEventData.time}].", IPA.Logging.Logger.Level.Error);
            logger.Log(e, IPA.Logging.Logger.Level.Error);
        }

        public static void LogFailure(HeckLogger logger, Exception e, BeatmapObjectData beatmapObjectData)
        {
            logger.Log($"Could not parse custom data for object [{beatmapObjectData.GetType().Name}] at [{beatmapObjectData.time}].", IPA.Logging.Logger.Level.Error);
            logger.Log(e, IPA.Logging.Logger.Level.Error);
        }

        internal static void InvokeDeserializeBeatmapData(bool isMultiplayer, CustomBeatmapData customBeatmapData, TrackBuilder trackBuilder)
        {
            IEnumerable<CustomEventData> customEventsData = customBeatmapData.customEventsData;
            IDictionary<string, CustomEventData>? eventDefinitions = customBeatmapData.customData.Get<IDictionary<string, CustomEventData>>("eventDefinitions");
            if (eventDefinitions != null)
            {
                customEventsData = customEventsData.Concat(eventDefinitions.Values);
            }
            else
            {
                Plugin.Logger.Log("Failed to load eventDefinitions.", IPA.Logging.Logger.Level.Error);
            }

            DeserializeBeatmapEventArgs eventArgs = new DeserializeBeatmapEventArgs(isMultiplayer, customBeatmapData, trackBuilder, customEventsData, customBeatmapData.beatmapEventsData, customBeatmapData.beatmapObjectsData);

            BuildTracks?.Invoke(eventArgs);

            DeserializeBeatmapData?.Invoke(eventArgs);
        }

        private static TFinalType? TryGetCustomData<TObjectType, TDataType, TFinalType>(this IDictionary<TObjectType, TDataType> dictionary, TObjectType key)
        {
            if (dictionary.TryGetValue(key, out TDataType customData))
            {
                if (customData is TFinalType t)
                {
                    return t;
                }
                else
                {
                    throw new InvalidOperationException($"Custom data was not of correct type. Expected: [{typeof(TFinalType).Name}], was: [{key?.GetType().Name}].");
                }
            }

            return default;
        }

        public class DeserializeBeatmapEventArgs : EventArgs
        {
            public DeserializeBeatmapEventArgs(
                bool isMultiplayer,
                CustomBeatmapData beatmapData,
                TrackBuilder trackBuilder,
                IEnumerable<CustomEventData> customEventDatas,
                IEnumerable<BeatmapEventData> beatmapEventDatas,
                IEnumerable<BeatmapObjectData> beatmapObjectDatas)
            {
                IsMultiplayer = isMultiplayer;
                BeatmapData = beatmapData;
                TrackBuilder = trackBuilder;
                CustomEventDatas = customEventDatas;
                BeatmapEventDatas = beatmapEventDatas;
                BeatmapObjectDatas = beatmapObjectDatas;
            }

            public bool IsMultiplayer { get; }

            public CustomBeatmapData BeatmapData { get; }

            public TrackBuilder TrackBuilder { get; }

            public IEnumerable<CustomEventData> CustomEventDatas { get; }

            public IEnumerable<BeatmapEventData> BeatmapEventDatas { get; }

            public IEnumerable<BeatmapObjectData> BeatmapObjectDatas { get; }
        }
    }
}
