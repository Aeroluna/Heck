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
                BasicPatch(difficultyBeatmap, customBeatmapData);
            }
        }

        internal static void Patch(IDifficultyBeatmap difficultyBeatmap, ref OverrideEnvironmentSettings? overrideEnvironmentSettings)
        {
            if (difficultyBeatmap.beatmapData is CustomBeatmapData customBeatmapData)
            {
                bool chromaRequirement = BasicPatch(difficultyBeatmap, customBeatmapData);
                if (chromaRequirement &&
                    ChromaConfig.Instance!.EnvironmentEnhancementsEnabled &&
                    (customBeatmapData.beatmapCustomData.Get<object>(Plugin.ENVIRONMENTREMOVAL) != null || (customBeatmapData.customData.Get<object>(Plugin.ENVIRONMENT) != null)))
                {
                    overrideEnvironmentSettings = null;
                }
            }
        }

        private static bool BasicPatch(IDifficultyBeatmap difficultyBeatmap, CustomBeatmapData customBeatmapData)
        {
            IEnumerable<string>? requirements = customBeatmapData.beatmapCustomData.Get<List<object>>("_requirements")?.Cast<string>();
            IEnumerable<string>? suggestions = customBeatmapData.beatmapCustomData.Get<List<object>>("_suggestions")?.Cast<string>();
            bool chromaRequirement = (requirements?.Contains(Plugin.REQUIREMENTNAME) ?? false) || (suggestions?.Contains(Plugin.REQUIREMENTNAME) ?? false);

            // please let me remove this shit
            bool legacyOverride = customBeatmapData.beatmapEventsData.Any(n => n.value >= LegacyLightHelper.RGB_INT_OFFSET);
            if (legacyOverride)
            {
                Plugin.Logger.Log("Legacy Chroma Detected...", IPA.Logging.Logger.Level.Warning);
                Plugin.Logger.Log("Please do not use Legacy Chroma Lights for new maps as it is deprecated and its functionality in future versions of Chroma cannot be guaranteed", IPA.Logging.Logger.Level.Warning);
            }

            ChromaController.ToggleChromaPatches((chromaRequirement || legacyOverride) && ChromaConfig.Instance!.CustomColorEventsEnabled);
            ChromaController.DoColorizerSabers = chromaRequirement && ChromaConfig.Instance!.CustomColorEventsEnabled;

            LightIDTableManager.SetEnvironment(difficultyBeatmap.GetEnvironmentInfo().serializedName);

            return chromaRequirement;
        }
    }
}
