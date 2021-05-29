namespace NoodleExtensions.HarmonyPatches
{
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using static NoodleExtensions.Plugin;

    [HarmonyPatch(typeof(BeatmapDataMirrorTransform))]
    [HarmonyPatch("CreateTransformedData")]
    internal static class BeatDataMirrorTransformCreateTransformedData
    {
        private static void Postfix(IReadonlyBeatmapData __result)
        {
            foreach (BeatmapEventData beatmapEventData in __result.beatmapEventsData)
            {
                if (beatmapEventData.type.IsRotationEvent())
                {
                    if (beatmapEventData is CustomBeatmapEventData customData)
                    {
                        dynamic dynData = customData.customData;
                        float? rotation = (float?)Trees.at(dynData, ROTATION);

                        if (rotation.HasValue)
                        {
                            dynData._rotation = rotation * -1;
                        }
                    }
                }
            }
        }
    }
}
