namespace NoodleExtensions.HarmonyPatches
{
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using static NoodleExtensions.Plugin;

    [NoodlePatch(typeof(SpawnRotationProcessor))]
    [NoodlePatch("ProcessBeatmapEventData")]
    internal static class SpawnRotationProcessorProcessBeatmapEventData
    {
        private static bool Prefix(BeatmapEventData beatmapEventData, ref float ____rotation)
        {
            if (beatmapEventData.type.IsRotationEvent() && beatmapEventData is CustomBeatmapEventData customData)
            {
                dynamic dynData = customData.customData;
                float? rotation = (float?)Trees.at(dynData, ROTATION);

                if (rotation.HasValue)
                {
                    ____rotation += rotation.Value;
                    return false;
                }
            }

            return true;
        }
    }
}
