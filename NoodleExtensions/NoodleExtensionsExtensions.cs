namespace NoodleExtensions
{
    using System;
    using System.Collections.Generic;
    using CustomJSONData.CustomBeatmap;

    internal static class NoodleExtensionsExtensions
    {
        internal static Dictionary<string, object?> GetDataForObject(this BeatmapObjectData beatmapObjectData)
        {
            switch (beatmapObjectData)
            {
                case CustomObstacleData data:
                    return data.customData;

                case CustomNoteData data:
                    return data.customData;

                case CustomWaypointData data:
                    return data.customData;

                default:
                    throw new InvalidOperationException($"beatmapObjectdata was not of type CustomObstacleData, CustomNoteData, or CustomWaypointData. Was: {beatmapObjectData.GetType().FullName}.");
            }
        }
    }
}
