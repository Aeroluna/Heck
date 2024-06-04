using System.Linq;
using Chroma.EnvironmentEnhancement.Saved;
using Chroma.Lighting;
using Chroma.Settings;
using CustomJSONData.CustomBeatmap;
using Heck;
using static Chroma.ChromaController;

namespace Chroma.Modules
{
    [Module("ChromaEnvironment", 2, LoadType.Active, new[] { "ChromaColorizer" })]
    [ModulePatcher(HARMONY_ID + "Environment", PatchType.Environment)]
    internal class FeaturesModule : IModule
    {
        private readonly Config _config;

        private FeaturesModule(Config config)
        {
            _config = config;
        }

        internal bool Active { get; private set; }

        [ModuleCallback]
        private void Callback(bool value)
        {
            Active = value;
        }

        [ModuleCondition]
        private bool Condition(
            Capabilities capabilities,
            IDifficultyBeatmap difficultyBeatmap)
        {
            bool chromaRequirement = capabilities.Requirements.Contains(CAPABILITY) || capabilities.Suggestions.Contains(CAPABILITY);

            // please let me remove this shit
            bool legacyOverride = difficultyBeatmap is CustomDifficultyBeatmap { beatmapSaveData: CustomBeatmapSaveData customBeatmapSaveData }
                                  && customBeatmapSaveData.basicBeatmapEvents.Any(n => n.value >= LegacyLightHelper.RGB_INT_OFFSET);

            bool customEnvironment = _config.CustomEnvironmentEnabled && (SavedEnvironmentLoader.Instance.SavedEnvironment?.Features.UseChromaEvents ?? false);

            // ReSharper disable once InvertIf
            if (legacyOverride)
            {
                Plugin.Log.Warn("Legacy Chroma Detected...");
                Plugin.Log.Warn("Please do not use Legacy Chroma Lights for new maps as it is deprecated and its functionality in future versions of Chroma cannot be guaranteed");
            }

            return (chromaRequirement || legacyOverride || customEnvironment) && !_config.ChromaEventsDisabled;
        }
    }
}
