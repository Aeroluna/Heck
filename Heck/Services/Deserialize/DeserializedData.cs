using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CustomJSONData.CustomBeatmap;

namespace Heck.Deserialize;

public class DeserializedData
{
    internal DeserializedData(
        Dictionary<CustomEventData, ICustomEventCustomData> customEventCustomDatas,
        Dictionary<BeatmapEventData, IEventCustomData> eventCustomDatas,
        Dictionary<BeatmapObjectData, IObjectCustomData> objectCustomDatas)
    {
        CustomEventCustomDatas = customEventCustomDatas;
        EventCustomDatas = eventCustomDatas;
        ObjectCustomDatas = objectCustomDatas;
    }

    public Dictionary<CustomEventData, ICustomEventCustomData> CustomEventCustomDatas { get; private set; }

    public Dictionary<BeatmapEventData, IEventCustomData> EventCustomDatas { get; private set; }

    public Dictionary<BeatmapObjectData, IObjectCustomData> ObjectCustomDatas { get; private set; }

    public bool Resolve<T>(CustomEventData customEventData, [NotNullWhen(true)] out T? result)
        where T : ICustomEventCustomData
    {
        return Resolve(CustomEventCustomDatas, customEventData, out result);
    }

    public bool Resolve<T>(BeatmapEventData beatmapEventData, [NotNullWhen(true)] out T? result)
        where T : IEventCustomData
    {
        return Resolve(EventCustomDatas, beatmapEventData, out result);
    }

    public bool Resolve<T>(BeatmapObjectData beatmapObjectData, [NotNullWhen(true)] out T? result)
        where T : IObjectCustomData
    {
        return Resolve(ObjectCustomDatas, beatmapObjectData, out result);
    }

    internal void RegisterNewObject(BeatmapObjectData beatmapObjectData, IObjectCustomData objectCustomData)
    {
        ObjectCustomDatas.Add(beatmapObjectData, objectCustomData);
    }

    // HIGHLY ILLEGAL!!!!
    internal void Remap(DeserializedData source)
    {
        CustomEventCustomDatas = source.CustomEventCustomDatas;
        EventCustomDatas = source.EventCustomDatas;
        ObjectCustomDatas = source.ObjectCustomDatas;
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
                throw new InvalidOperationException(
                    $"Custom data was not of correct type. Expected: [{typeof(TResultType).Name}], was: [{customData.GetType().Name}].");
            }

            result = t;
            return true;
        }

        result = default;
        return false;
    }
}
