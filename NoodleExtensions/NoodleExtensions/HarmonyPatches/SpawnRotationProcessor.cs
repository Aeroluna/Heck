using BS_Utils.Utilities;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using System.Linq;
using UnityEngine;

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

            float? _rotation = null;

            // CustomJSONData
            if (Plugin.NoodleExtensionsActive && beatmapEventData is CustomBeatmapEventData customData)
            {
                dynamic dynData = customData.customData;
                _rotation = (float?)Trees.at(dynData, "_rotation");
            }

            // Mapping Extensions Legacy Support
            if (Plugin.MappingExtensionsActive && _rotation.HasValue) _rotation = beatmapEventData.value - 1360;

            if (_rotation.HasValue) ____rotation = _rotation.Value;
        }
    }
}