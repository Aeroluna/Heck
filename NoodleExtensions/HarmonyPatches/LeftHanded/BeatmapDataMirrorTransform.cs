using System.Collections.Generic;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using JetBrains.Annotations;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.LeftHanded
{
    [HarmonyPatch(typeof(BeatmapDataMirrorTransform))]
    [HarmonyPatch("CreateTransformedData")]
    internal static class BeatDataMirrorTransformCreateTransformedData
    {
        [UsedImplicitly]
        private static void Postfix(IReadonlyBeatmapData __result)
        {
            foreach (BeatmapEventData beatmapEventData in __result.beatmapEventsData)
            {
                if (!beatmapEventData.type.IsRotationEvent() || beatmapEventData is not CustomBeatmapEventData customData)
                {
                    continue;
                }

                Dictionary<string, object?> dynData = customData.customData;
                float? rotation = dynData.Get<float?>(ROTATION);

                if (rotation.HasValue)
                {
                    dynData["_rotation"] = rotation * -1;
                }
            }
        }
    }
}
