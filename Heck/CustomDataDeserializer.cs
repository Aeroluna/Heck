namespace Heck
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;

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
        public static event Action<bool, IEnumerable<CustomEventData>, CustomBeatmapData>? OnDeserializeCustomEventDatas;

        public static event Action<bool, IEnumerable<BeatmapEventData>, CustomBeatmapData>? OnDeserializeBeatmapEventDatas;

        public static event Action<bool, IEnumerable<BeatmapObjectData>, CustomBeatmapData>? OnDeserializeBeatmapObjectDatas;

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
            logger.Log($"Could not parse custom data for custom event {customEventData.type} at {customEventData.time}.", IPA.Logging.Logger.Level.Error);
            logger.Log(e, IPA.Logging.Logger.Level.Error);
        }

        public static void LogFailure(HeckLogger logger, Exception e, BeatmapEventData beatmapEventData)
        {
            logger.Log($"Could not parse custom data for event {beatmapEventData.type} at {beatmapEventData.time}.", IPA.Logging.Logger.Level.Error);
            logger.Log(e, IPA.Logging.Logger.Level.Error);
        }

        public static void LogFailure(HeckLogger logger, Exception e, BeatmapObjectData beatmapObjectData)
        {
            logger.Log($"Could not parse custom data for object {beatmapObjectData.GetType().Name} at {beatmapObjectData.time}.", IPA.Logging.Logger.Level.Error);
            logger.Log(e, IPA.Logging.Logger.Level.Error);
        }

        internal static void DeserializeBeatmapData(bool isMultiplayer, CustomBeatmapData customBeatmapData)
        {
            OnDeserializeBeatmapEventDatas?.Invoke(isMultiplayer, customBeatmapData.beatmapEventsData, customBeatmapData);
            OnDeserializeBeatmapObjectDatas?.Invoke(isMultiplayer, customBeatmapData.beatmapObjectsData, customBeatmapData);

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

            OnDeserializeCustomEventDatas?.Invoke(isMultiplayer, customEventsData, customBeatmapData);
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
    }
}
