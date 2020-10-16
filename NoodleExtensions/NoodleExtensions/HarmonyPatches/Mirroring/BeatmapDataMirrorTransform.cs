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
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(IReadonlyBeatmapData __result)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
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
