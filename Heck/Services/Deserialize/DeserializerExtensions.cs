using System;
using BepInEx.Logging;
using CustomJSONData.CustomBeatmap;

namespace Heck
{
    public static class DeserializerExtensions
    {
        public static void LogFailure(this ManualLogSource logger, Exception e, CustomEventData customEventData)
        {
            logger.LogError($"Could not parse custom data for custom event [{customEventData.eventType}] at [{customEventData.time}].");
            logger.LogError(e);
        }

        public static void LogFailure(this ManualLogSource logger, Exception e, BeatmapEventData beatmapEventData)
        {
            logger.LogError($"Could not parse custom data for event [{beatmapEventData.GetType().Name}] at [{beatmapEventData.time}].");
            logger.LogError(e);
        }

        public static void LogFailure(this ManualLogSource logger, Exception e, BeatmapObjectData beatmapObjectData)
        {
            logger.LogError($"Could not parse custom data for object [{beatmapObjectData.GetType().Name}] at [{beatmapObjectData.time}].");
            logger.LogError(e);
        }
    }
}
