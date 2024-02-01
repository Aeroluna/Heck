using System;
using CustomJSONData.CustomBeatmap;
using IPA.Logging;
using static Heck.HeckController;

namespace Heck
{
    public static class DeserializerExtensions
    {
        [Obsolete("Use IDifficultyBeatmap overload", true)]
        public static void LogFailure(this HeckLogger logger, Exception e, CustomEventData customEventData)
        {
            logger.Log($"Could not parse custom data for custom event [{customEventData.eventType}] at [{customEventData.time}].", Logger.Level.Error);
            logger.Log(e, Logger.Level.Error);
        }

        [Obsolete("Use IDifficultyBeatmap overload", true)]
        public static void LogFailure(this HeckLogger logger, Exception e, BeatmapEventData beatmapEventData)
        {
            logger.Log($"Could not parse custom data for event [{beatmapEventData.GetType().Name}] at [{beatmapEventData.time}].", Logger.Level.Error);
            logger.Log(e, Logger.Level.Error);
        }

        [Obsolete("Use IDifficultyBeatmap overload", true)]
        public static void LogFailure(this HeckLogger logger, Exception e, BeatmapObjectData beatmapObjectData)
        {
            logger.Log($"Could not parse custom data for object [{beatmapObjectData.GetType().Name}] at [{beatmapObjectData.time}].", Logger.Level.Error);
            logger.Log(e, Logger.Level.Error);
        }

        public static void LogFailure(this HeckLogger logger, Exception e, CustomEventData customEventData, IDifficultyBeatmap difficultyBeatmap)
        {
            logger.Log($"Could not parse custom data for custom event [{customEventData.eventType}] at [{GetBeatTime(customEventData, difficultyBeatmap)}].", Logger.Level.Error);
            logger.Log(e, Logger.Level.Error);
        }

        public static void LogFailure(this HeckLogger logger, Exception e, BeatmapEventData beatmapEventData, IDifficultyBeatmap difficultyBeatmap)
        {
            logger.Log($"Could not parse custom data for event [{beatmapEventData.GetType().Name}] at [{GetBeatTime(beatmapEventData, difficultyBeatmap)}].", Logger.Level.Error);
            logger.Log(e, Logger.Level.Error);
        }

        public static void LogFailure(this HeckLogger logger, Exception e, BeatmapObjectData beatmapObjectData, IDifficultyBeatmap difficultyBeatmap)
        {
            logger.Log($"Could not parse custom data for object [{beatmapObjectData.GetType().Name}] at [{GetBeatTime(beatmapObjectData, difficultyBeatmap)}].", Logger.Level.Error);
            logger.Log(e, Logger.Level.Error);
        }

        private static string GetBeatTime(BeatmapDataItem item, IDifficultyBeatmap difficultyBeatmap)
        {
            float beat = (item.time / 60f) * difficultyBeatmap.level.beatsPerMinute;
            return $"{beat}";
        }
    }
}
