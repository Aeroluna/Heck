using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using static NoodleExtensions.Plugin;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(SpawnRotationProcessor))]
    [HarmonyPatch("ProcessBeatmapEventData")]
    internal class SpawnRotationProcessorProcessBeatmapEventData
    {
        private static bool Prefix(BeatmapEventData beatmapEventData, ref float ____rotation)
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