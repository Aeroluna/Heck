// i'll reimplement this the first time someone complains to me
/*using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches
{
    // i still dont even know if this works, but must maintain feature parity with mapping extensions!
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(SpawnRotationProcessor))]
    internal static class PrecisionRotationEvent
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SpawnRotationProcessor.ProcessBeatmapEventData))]
        private static bool Prefix(BeatmapEventData beatmapEventData, ref float ____rotation)
        {
            if (!beatmapEventData.type.IsRotationEvent() ||
                beatmapEventData is not CustomBeatmapEventData customData)
            {
                return true;
            }

            float? rotation = customData.customData.Get<float?>(ROTATION);

            if (!rotation.HasValue)
            {
                return true;
            }

            ____rotation += rotation.Value;
            return false;
        }
    }
}*/
