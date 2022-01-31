using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;

namespace NoodleExtensions.Extras
{
    internal static class NoodleExtensionsExtensions
    {
        internal static Dictionary<string, object?> GetDataForObject(this BeatmapObjectData beatmapObjectData)
        {
            return beatmapObjectData switch
            {
                CustomObstacleData data => data.customData,
                CustomNoteData data => data.customData,
                CustomWaypointData data => data.customData,
                _ => throw new InvalidOperationException(
                    $"beatmapObjectdata was not of type CustomObstacleData, CustomNoteData, or CustomWaypointData. Was: {beatmapObjectData.GetType().FullName}.")
            };
        }
    }
}
