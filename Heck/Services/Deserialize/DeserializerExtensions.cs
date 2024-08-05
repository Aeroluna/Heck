using System;
using CustomJSONData.CustomBeatmap;
using IPA.Logging;

namespace Heck.Deserialize;

public static class DeserializerExtensions
{
    public static void DeserializeFailure(this Logger logger, Exception e, CustomEventData customEventData, float bpm)
    {
        logger.Error(
            $"Could not parse custom data for custom event [{customEventData.eventType}] at [{GetBeatTime(customEventData, bpm)}]");
        logger.Error(e);
    }

    public static void DeserializeFailure(this Logger logger, Exception e, BeatmapEventData beatmapEventData, float bpm)
    {
        logger.Error(
            $"Could not parse custom data for event [{beatmapEventData.GetType().Name}] at [{GetBeatTime(beatmapEventData, bpm)}]");
        logger.Error(e);
    }

    public static void DeserializeFailure(
        this Logger logger,
        Exception e,
        BeatmapObjectData beatmapObjectData,
        float bpm)
    {
        logger.Error(
            $"Could not parse custom data for object [{beatmapObjectData.GetType().Name}] at [{GetBeatTime(beatmapObjectData, bpm)}]");
        logger.Error(e);
    }

    private static string GetBeatTime(BeatmapDataItem item, float bpm)
    {
        float beat = (item.time / 60f) * bpm;
        return $"{beat}";
    }
}
