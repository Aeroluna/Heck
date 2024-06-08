using System.Linq;
using Chroma.EnvironmentEnhancement.Saved;
using Chroma.Lighting;
using Chroma.Settings;
using CustomJSONData.CustomBeatmap;
using Heck;
using SiraUtil.Logging;
using static Chroma.ChromaController;

namespace Chroma.Modules
{
    [Module(ID, 3, LoadType.Active, new[] { "ChromaColorizer", "ChromaEnvironment" })]
    [ModulePatcher(HARMONY_ID + "Features", PatchType.Features)]
    internal class FeaturesModule : IModule
    {
        private readonly SiraLog _log;
        private readonly Config _config;

        private FeaturesModule(SiraLog log, Config config)
        {
            _log = log;
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
                _log.Warn("Legacy Chroma Detected...");
                _log.Warn("Please do not use Legacy Chroma Lights for new maps as it is deprecated and its functionality in future versions of Chroma cannot be guaranteed");
            }

            return (chromaRequirement || legacyOverride || customEnvironment) && !_config.ChromaEventsDisabled;
        }
    }
}
