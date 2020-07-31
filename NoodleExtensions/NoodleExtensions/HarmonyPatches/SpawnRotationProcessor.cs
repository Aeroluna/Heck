namespace NoodleExtensions.HarmonyPatches
{
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using static NoodleExtensions.Plugin;

    [NoodlePatch(typeof(SpawnRotationProcessor))]
    [NoodlePatch("ProcessBeatmapEventData")]
    internal static class SpawnRotationProcessorProcessBeatmapEventData
    {
#pragma warning disable SA1313
        private static bool Prefix(BeatmapEventData beatmapEventData, ref float ____rotation)
#pragma warning restore SA1313
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
