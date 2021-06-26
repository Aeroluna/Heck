namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
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
                        Dictionary<string, object> dynData = customData.customData;
                        float? rotation = dynData.Get<float?>(ROTATION);

                        if (rotation.HasValue)
                        {
                            dynData["_rotation"] = rotation * -1;
                        }
                    }
                }
            }
        }
    }
}
