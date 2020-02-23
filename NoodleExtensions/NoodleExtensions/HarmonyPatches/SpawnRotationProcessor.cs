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
        public static void Postfix(bool __result, BeatmapEventData beatmapEventData, ref float ____rotation)
        {
            if (!__result) return;

            // CustomJSONData
            if (NoodleExtensionsActive && !MappingExtensionsActive && beatmapEventData is CustomBeatmapEventData customData)
            {
                dynamic dynData = customData.customData;
                float? _rotation = (float?)Trees.at(dynData, "_rotation");

                if (_rotation.HasValue) ____rotation = _rotation.Value;
            }
        }
    }
}