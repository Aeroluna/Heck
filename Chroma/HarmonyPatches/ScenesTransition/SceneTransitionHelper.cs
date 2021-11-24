using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.Lighting;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using IPA.Logging;
using static Chroma.ChromaController;

namespace Chroma.HarmonyPatches.ScenesTransition
{
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
            if (difficultyBeatmap.beatmapData is not CustomBeatmapData customBeatmapData)
            {
                return;
            }

            bool chromaRequirement = BasicPatch(difficultyBeatmap, customBeatmapData);

            try
            {
                if (chromaRequirement &&
                    !ChromaConfig.Instance.EnvironmentEnhancementsDisabled &&
                    ((customBeatmapData.beatmapCustomData.Get<List<object>>(ENVIRONMENT_REMOVAL)?.Any() ?? false) || (customBeatmapData.customData.Get<List<object>>(ENVIRONMENT)?.Any() ?? false)))
                {
                    overrideEnvironmentSettings = null;
                }
            }
            catch (Exception e)
            {
                // for all you sussy bakas that use _environment to just force the environment: FUCK YOU!!!!!!!!!!!!!
                Log.Logger.Log(e, Logger.Level.Error);
            }
        }

        private static bool BasicPatch(IDifficultyBeatmap difficultyBeatmap, CustomBeatmapData customBeatmapData)
        {
            IEnumerable<string>? requirements = customBeatmapData.beatmapCustomData.Get<List<object>>("_requirements")?.Cast<string>();
            IEnumerable<string>? suggestions = customBeatmapData.beatmapCustomData.Get<List<object>>("_suggestions")?.Cast<string>();
            bool chromaRequirement = (requirements?.Contains(CAPABILITY) ?? false) || (suggestions?.Contains(CAPABILITY) ?? false);

            // please let me remove this shit
            bool legacyOverride = customBeatmapData.beatmapEventsData.Any(n => n.value >= LegacyLightHelper.RGB_INT_OFFSET);
            if (legacyOverride)
            {
                Log.Logger.Log("Legacy Chroma Detected...", Logger.Level.Warning);
                Log.Logger.Log("Please do not use Legacy Chroma Lights for new maps as it is deprecated and its functionality in future versions of Chroma cannot be guaranteed", Logger.Level.Warning);
            }

            ToggleChromaPatches((chromaRequirement || legacyOverride) && !ChromaConfig.Instance.ChromaEventsDisabled);
            DoColorizerSabers = chromaRequirement && !ChromaConfig.Instance.ChromaEventsDisabled;

            LightIDTableManager.SetEnvironment(difficultyBeatmap.GetEnvironmentInfo().serializedName);

            return chromaRequirement;
        }
    }
}
