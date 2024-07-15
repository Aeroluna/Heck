using System.Linq;
using Chroma.EnvironmentEnhancement.Saved;
using Chroma.Settings;
using Heck;
using Heck.Module;
using SiraUtil.Logging;
using static Chroma.ChromaController;
#if !LATEST
using Chroma.Lighting;
using CustomJSONData.CustomBeatmap;
#endif

namespace Chroma.Modules
{
    [Module(ID, 3, LoadType.Active, new[] { "ChromaColorizer", "ChromaEnvironment" })]
    [ModulePatcher(HARMONY_ID + "Features", PatchType.Features)]
    internal class FeaturesModule : IModule
    {
        private readonly SiraLog _log;
        private readonly Config _config;
        private readonly SavedEnvironmentLoader _savedEnvironmentLoader;

        private FeaturesModule(SiraLog log, Config config, SavedEnvironmentLoader savedEnvironmentLoader)
        {
            _log = log;
            _config = config;
            _savedEnvironmentLoader = savedEnvironmentLoader;
        }

        internal bool Active { get; private set; }

        [ModuleCallback]
        private void Callback(bool value)
        {
            Active = value;
        }

        [ModuleCondition]
        private bool Condition(
#if !LATEST
            IDifficultyBeatmap difficultyBeatmap,
#endif
            Capabilities capabilities)
        {
            bool chromaRequirement = capabilities.Requirements.Contains(CAPABILITY) || capabilities.Suggestions.Contains(CAPABILITY);
            bool customEnvironment = _config.CustomEnvironmentEnabled && (_savedEnvironmentLoader.SavedEnvironment?.Features.UseChromaEvents ?? false);

            // the only alternative i see is postfixing LevelScenesTransitionSetupDataSO.BeforeScenesWillBeActivatedAsync
            // that would push module activation further up, after the beatmapdata is created, allowing us to read its customdata and events
            // unfortunately that would be after environment info is loaded so it wouldnt be possible to override the OverrideEnvironmentSettings for the environment module
            // for now we will listen to for the legacy capability, but this will miss maps who dont set requirements unfortunately
            // please let me remove this shit
#if LATEST
            bool legacyOverride = capabilities.Requirements.Contains(LEGACY_CAPABILITY) || capabilities.Suggestions.Contains(LEGACY_CAPABILITY);
#else
            bool legacyOverride = difficultyBeatmap is CustomDifficultyBeatmap { beatmapSaveData: Version3CustomBeatmapSaveData customBeatmapSaveData }
                                  && customBeatmapSaveData.basicBeatmapEvents.Any(n => n.value >= LegacyLightHelper.RGB_INT_OFFSET);
#endif

            // ReSharper disable once InvertIf
            if (legacyOverride)
            {
                _log.Warn("Legacy Chroma Detected...");
                _log.Warn("Please do not use Legacy Chroma Lights for new maps as it is deprecated and its functionality in future versions of Chroma cannot be guaranteed");
            }

            return (chromaRequirement ||
                    legacyOverride ||
                    customEnvironment) && !_config.ChromaEventsDisabled;
        }
    }
}
