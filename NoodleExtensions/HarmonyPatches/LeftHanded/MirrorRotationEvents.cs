/*using System.Collections.Generic;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.LeftHanded
{
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(BeatmapDataMirrorTransform))]
    internal static class MirrorRotationEvents
    {
        [HarmonyPostfix]
        [HarmonyPatch("CreateTransformedData")]
        private static void Postfix(IReadonlyBeatmapData __result)
        {
            foreach (BeatmapEventData beatmapEventData in __result.beatmapEventsData)
            {
                if (!beatmapEventData.type.IsRotationEvent() || beatmapEventData is not CustomBeatmapEventData customData)
                {
                    continue;
                }

                CustomData customData = customData.customData;
                float? rotation = customData.Get<float?>(ROTATION);

                if (rotation.HasValue)
                {
                    customData["_rotation"] = rotation * -1;
                }
            }
        }
    }
}*/
