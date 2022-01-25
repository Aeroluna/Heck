using System;
using CustomJSONData.CustomBeatmap;
using IPA.Logging;

namespace Heck
{
    public static class DeserializerExtensions
    {
        public static void LogFailure(this HeckLogger logger, Exception e, CustomEventData customEventData)
        {
            logger.Log($"Could not parse custom data for custom event [{customEventData.type}] at [{customEventData.time}].", Logger.Level.Error);
            logger.Log(e, Logger.Level.Error);
        }

        public static void LogFailure(this HeckLogger logger, Exception e, BeatmapEventData beatmapEventData)
        {
            logger.Log($"Could not parse custom data for event [{beatmapEventData.type}] at [{beatmapEventData.time}].", Logger.Level.Error);
            logger.Log(e, Logger.Level.Error);
        }

        public static void LogFailure(this HeckLogger logger, Exception e, BeatmapObjectData beatmapObjectData)
        {
            logger.Log($"Could not parse custom data for object [{beatmapObjectData.GetType().Name}] at [{beatmapObjectData.time}].", Logger.Level.Error);
            logger.Log(e, Logger.Level.Error);
        }
    }
}
