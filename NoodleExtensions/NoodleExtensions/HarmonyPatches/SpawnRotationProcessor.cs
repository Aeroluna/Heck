using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPriority(Priority.Normal)]
    [HarmonyPatch(typeof(SpawnRotationProcessor))]
    [HarmonyPatch("ProcessBeatmapEventData")]
    internal class SpawnRotationProcessorProcessBeatmapEventData
    {
        public static bool Prefix(BeatmapEventData beatmapEventData, ref float ____rotation)
        {
            if (!beatmapEventData.type.IsRotationEvent()) return true;

            if (NoodleExtensionsActive && !MappingExtensionsActive && beatmapEventData is CustomBeatmapEventData customData)
            {
                dynamic dynData = customData.customData;
                float? _rotation = (float?)Trees.at(dynData, ROTATION);

                if (_rotation.HasValue)
                {
                    ____rotation += _rotation.Value;
                    return false;
                }
            }

            return true;
        }
    }
}