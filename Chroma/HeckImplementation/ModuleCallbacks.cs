using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.Lighting;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck;
using IPA.Logging;
using static Chroma.ChromaController;

namespace Chroma
{
    internal class ModuleCallbacks
    {
        [ModuleCallback(PatchType.Colorizer)]
        private static void ToggleColorizer(bool value)
        {
            Deserializer.Enabled = value;
            ColorizerPatcher.Enabled = value;
        }

        [ModuleCondition(PatchType.Features)]
        private static bool ConditionFeatures(
            Capabilities capabilities,
            IDifficultyBeatmap difficultyBeatmap,
            ModuleManager.ModuleArgs moduleArgs)
        {
            bool chromaRequirement = capabilities.Requirements.Contains(CAPABILITY) || capabilities.Suggestions.Contains(CAPABILITY);

            // please let me remove this shit
            bool legacyOverride = difficultyBeatmap.beatmapData.beatmapEventsData.Any(n => n.value >= LegacyLightHelper.RGB_INT_OFFSET);
            if (legacyOverride)
            {
                Log.Logger.Log("Legacy Chroma Detected...", Logger.Level.Warning);
                Log.Logger.Log("Please do not use Legacy Chroma Lights for new maps as it is deprecated and its functionality in future versions of Chroma cannot be guaranteed", Logger.Level.Warning);
            }

            LightIDTableManager.SetEnvironment(difficultyBeatmap.GetEnvironmentInfo().serializedName);

            return (chromaRequirement || legacyOverride) && !ChromaConfig.Instance.ChromaEventsDisabled;
        }

        [ModuleCallback(PatchType.Features)]
        private static void ToggleFeatures(
            bool value,
            IDifficultyBeatmap difficultyBeatmap,
            ModuleManager.ModuleArgs moduleArgs)
        {
            FeaturesPatcher.Enabled = value;

            try
            {
                CustomBeatmapData customBeatmapData = (CustomBeatmapData)difficultyBeatmap.beatmapData;

                if (value &&
                    !ChromaConfig.Instance.EnvironmentEnhancementsDisabled &&
                    ((customBeatmapData.beatmapCustomData.Get<List<object>>(ENVIRONMENT_REMOVAL)?.Any() ?? false) || (customBeatmapData.customData.Get<List<object>>(ENVIRONMENT)?.Any() ?? false)))
                {
                    moduleArgs.OverrideEnvironmentSettings = null;
                }
            }
            catch (Exception e)
            {
                // for all you sussy bakas that use _environment to just force the environment: FUCK YOU!!!!!!!!!!!!!
                Log.Logger.Log(e, Logger.Level.Error);
            }
        }
    }
}
