namespace NoodleExtensions
{
    using System.Collections.Generic;
    using CustomJSONData.CustomBeatmap;

    internal static class NoodleExtensionsExtensions
    {
        internal static Dictionary<string, object> GetDataForObject(this BeatmapObjectData beatmapObjectData)
        {
            Dictionary<string, object> dynData = null;
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
                    Plugin.Logger.Log("beatmapObjectdata was not of type CustomObstacleData, CustomNoteData, or CustomWaypointData.", IPA.Logging.Logger.Level.Error);
                    break;
            }

            return dynData;
        }
    }
}
