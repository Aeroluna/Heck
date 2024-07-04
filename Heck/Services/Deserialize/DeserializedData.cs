using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CustomJSONData.CustomBeatmap;

namespace Heck.Deserialize
{
    public class DeserializedData
    {
        private Dictionary<CustomEventData, ICustomEventCustomData> _customEventCustomDatas;
        private Dictionary<BeatmapEventData, IEventCustomData> _eventCustomDatas;
        private Dictionary<BeatmapObjectData, IObjectCustomData> _objectCustomDatas;

        internal DeserializedData(
            Dictionary<CustomEventData, ICustomEventCustomData> customEventCustomDatas,
            Dictionary<BeatmapEventData, IEventCustomData> eventCustomDatas,
            Dictionary<BeatmapObjectData, IObjectCustomData> objectCustomDatas)
        {
            _customEventCustomDatas = customEventCustomDatas;
            _eventCustomDatas = eventCustomDatas;
            _objectCustomDatas = objectCustomDatas;
        }

        public bool Resolve<T>(CustomEventData customEventData, [NotNullWhen(true)] out T? result)
            where T : ICustomEventCustomData
        {
            return Resolve(_customEventCustomDatas, customEventData, out result);
        }

        public bool Resolve<T>(BeatmapEventData beatmapEventData, [NotNullWhen(true)] out T? result)
            where T : IEventCustomData
        {
            return Resolve(_eventCustomDatas, beatmapEventData, out result);
        }

        public bool Resolve<T>(BeatmapObjectData beatmapObjectData, [NotNullWhen(true)] out T? result)
            where T : IObjectCustomData
        {
            return Resolve(_objectCustomDatas, beatmapObjectData, out result);
        }

        internal void RegisterNewObject(BeatmapObjectData beatmapObjectData, IObjectCustomData objectCustomData)
        {
            _objectCustomDatas.Add(beatmapObjectData, objectCustomData);
        }

        // HIGHLY ILLEGAL!!!!
        internal void Remap(DeserializedData source)
        {
            _customEventCustomDatas = source._customEventCustomDatas;
            _eventCustomDatas = source._eventCustomDatas;
            _objectCustomDatas = source._objectCustomDatas;
        }

        private static bool Resolve<TBaseData, TResultType, TResultData>(
            Dictionary<TBaseData, TResultType> dictionary,
            TBaseData baseData,
            [NotNullWhen(true)] out TResultData? result)
            where TResultData : TResultType
        {
            if (dictionary.TryGetValue(baseData, out TResultType customData) && customData != null)
            {
                if (customData is not TResultData t)
                {
                    throw new InvalidOperationException($"Custom data was not of correct type. Expected: [{typeof(TResultType).Name}], was: [{customData.GetType().Name}].");
                }

                result = t;
                return true;
            }

            result = default;
            return false;
        }
    }
}
