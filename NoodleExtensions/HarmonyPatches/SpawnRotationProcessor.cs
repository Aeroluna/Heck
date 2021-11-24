using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck;
using JetBrains.Annotations;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches
{
    // i still dont even know if this works, but must maintain feature parity with mapping extensions!
    [HeckPatch(typeof(SpawnRotationProcessor))]
    [HeckPatch("ProcessBeatmapEventData")]
    internal static class SpawnRotationProcessorProcessBeatmapEventData
    {
        [UsedImplicitly]
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
}
