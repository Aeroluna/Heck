using CustomJSONData;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO),
        new Type[] { typeof(IDifficultyBeatmap), typeof(OverrideEnvironmentSettings), typeof(ColorScheme), typeof(GameplayModifiers),
            typeof(PlayerSpecificSettings), typeof(PracticeSettings), typeof(string), typeof(bool)})]
    [HarmonyPatch("Init")]
    internal class StandardLevelScenesTransitionSetupDataSOInit
    {
        private static void Postfix(IDifficultyBeatmap difficultyBeatmap)
        {
            if (difficultyBeatmap.beatmapData is CustomJSONData.CustomBeatmap.CustomBeatmapData customBeatmapData)
            {
                IEnumerable<string> requirements = ((List<object>)Trees.at(customBeatmapData.beatmapCustomData, "_requirements"))?.Cast<string>();
                IEnumerable<string> suggestions = ((List<object>)Trees.at(customBeatmapData.beatmapCustomData, "_suggestions"))?.Cast<string>();
                ChromaBehaviour.LightingRegistered = (requirements?.Contains(Plugin.REQUIREMENT_NAME) ?? false) || (suggestions?.Contains(Plugin.REQUIREMENT_NAME) ?? false);
            }

            ChromaBehaviour.LegacyOverride = difficultyBeatmap.beatmapData.beatmapEventData.Any(n => n.value >= Events.ChromaLegacyRGBEvent.RGB_INT_OFFSET);
        }
    }
}