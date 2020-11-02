namespace Chroma.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using Chroma.Settings;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;

    internal static class SceneTransitionHelper
    {
        internal static void Patch(IDifficultyBeatmap difficultyBeatmap)
        {
            if (difficultyBeatmap.beatmapData is CustomBeatmapData customBeatmapData)
            {
                BasicPatch(customBeatmapData);
            }
        }

        internal static void Patch(IDifficultyBeatmap difficultyBeatmap, ref OverrideEnvironmentSettings overrideEnvironmentSettings)
        {
            if (difficultyBeatmap.beatmapData is CustomBeatmapData customBeatmapData)
            {
                bool chromaRequirement = BasicPatch(customBeatmapData);
                if (chromaRequirement && ChromaConfig.Instance.EnvironmentEnhancementsEnabled && Trees.at(customBeatmapData.beatmapCustomData, "_environmentRemoval") != null)
                {
                    overrideEnvironmentSettings = null;
                }
            }
        }

        private static bool BasicPatch(CustomBeatmapData customBeatmapData)
        {
            IEnumerable<string> requirements = ((List<object>)Trees.at(customBeatmapData.beatmapCustomData, "_requirements"))?.Cast<string>();
            IEnumerable<string> suggestions = ((List<object>)Trees.at(customBeatmapData.beatmapCustomData, "_suggestions"))?.Cast<string>();
            bool chromaRequirement = (requirements?.Contains(Chroma.Plugin.REQUIREMENTNAME) ?? false) || (suggestions?.Contains(Chroma.Plugin.REQUIREMENTNAME) ?? false);

            // please let me remove this shit
            bool legacyOverride = customBeatmapData.beatmapEventsData.Any(n => n.value >= LegacyLightHelper.RGB_INT_OFFSET);
            if (legacyOverride)
            {
                ChromaLogger.Log("Legacy Chroma Detected...", IPA.Logging.Logger.Level.Warning);
                ChromaLogger.Log("Please do not use Legacy Chroma for new maps as it is deprecated and its functionality in future versions of Chroma cannot be guaranteed", IPA.Logging.Logger.Level.Warning);
            }

            ChromaController.ToggleChromaPatches((chromaRequirement || legacyOverride) && ChromaConfig.Instance.CustomColorEventsEnabled);
            ChromaController.DoColorizerSabers = chromaRequirement && ChromaConfig.Instance.CustomColorEventsEnabled;

            return chromaRequirement;
        }
    }
}
