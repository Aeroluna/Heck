using System;
using CustomJSONData.CustomBeatmap;
using IPA.Logging;

namespace Heck
{
    // TODO: fix namespace
    public static class DeserializerExtensions
    {
        public static void DeserializeFailure(this Logger logger, Exception e, CustomEventData customEventData, IDifficultyBeatmap difficultyBeatmap)
        {
            logger.Error($"Could not parse custom data for custom event [{customEventData.eventType}] at [{GetBeatTime(customEventData, difficultyBeatmap)}]");
            logger.Error(e);
        }

        public static void DeserializeFailure(this Logger logger, Exception e, BeatmapEventData beatmapEventData, IDifficultyBeatmap difficultyBeatmap)
        {
            logger.Error($"Could not parse custom data for event [{beatmapEventData.GetType().Name}] at [{GetBeatTime(beatmapEventData, difficultyBeatmap)}]");
            logger.Error(e);
        }

        public static void DeserializeFailure(this Logger logger, Exception e, BeatmapObjectData beatmapObjectData, IDifficultyBeatmap difficultyBeatmap)
        {
            logger.Error($"Could not parse custom data for object [{beatmapObjectData.GetType().Name}] at [{GetBeatTime(beatmapObjectData, difficultyBeatmap)}]");
            logger.Error(e);
        }

        private static string GetBeatTime(BeatmapDataItem item, IDifficultyBeatmap difficultyBeatmap)
        {
            float beat = (item.time / 60f) * difficultyBeatmap.level.beatsPerMinute;
            return $"{beat}";
        }
    }
}
