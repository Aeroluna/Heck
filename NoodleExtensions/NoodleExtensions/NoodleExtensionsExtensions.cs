namespace NoodleExtensions
{
    using System;
    using System.Collections.Generic;
    using CustomJSONData.CustomBeatmap;

    internal static class NoodleExtensionsExtensions
    {
        internal static Dictionary<string, object?> GetDataForObject(this BeatmapObjectData beatmapObjectData)
        {
            Dictionary<string, object?> dynData;
            switch (beatmapObjectData)
            {
                case CustomObstacleData data:
                    dynData = data.customData;
                    break;

                case CustomNoteData data:
                    dynData = data.customData;
                    break;

                case CustomWaypointData data:
                    dynData = data.customData;
                    break;

                default:
                    throw new InvalidOperationException($"beatmapObjectdata was not of type CustomObstacleData, CustomNoteData, or CustomWaypointData. Was: {beatmapObjectData.GetType().FullName}");
            }

            return dynData;
        }
    }
}
