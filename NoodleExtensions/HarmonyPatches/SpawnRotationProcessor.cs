namespace NoodleExtensions.HarmonyPatches
{
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using Heck;
    using static NoodleExtensions.Plugin;

    // i still dont even know if this works, but must maintain feature parity with mapping extensions!
    [HeckPatch(typeof(SpawnRotationProcessor))]
    [HeckPatch("ProcessBeatmapEventData")]
    internal static class SpawnRotationProcessorProcessBeatmapEventData
    {
        private static bool Prefix(BeatmapEventData beatmapEventData, ref float ____rotation)
        {
            if (beatmapEventData.type.IsRotationEvent() && beatmapEventData is CustomBeatmapEventData customData)
            {
                float? rotation = customData.customData.Get<float?>(ROTATION);

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
