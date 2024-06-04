using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using Chroma.EnvironmentEnhancement.Saved;
using JetBrains.Annotations;

namespace Chroma.Settings
{
    internal class ChromaSettingsUI : IDisposable
    {
        private const string NO_ENVIRONMENT = "None";

        private readonly Config _config;
        private readonly SavedEnvironmentLoader _savedEnvironmentLoader;

        // TODO: do a comparison instead of just always wiping the cache
        private readonly BeatmapDataCache _beatmapDataCache;

        [UsedImplicitly]
        [UIValue("environmentoptions")]
        private List<object?> _environmentOptions;

        private ChromaSettingsUI(Config config, SavedEnvironmentLoader savedEnvironmentLoader, BeatmapDataCache beatmapDataCache)
        {
            _config = config;
            _savedEnvironmentLoader = savedEnvironmentLoader;
            _beatmapDataCache = beatmapDataCache;
            _environmentOptions = _savedEnvironmentLoader.Environments.Keys.Cast<object?>().Prepend(null).ToList();

            GameplaySetup.instance.AddTab("Chroma", "Chroma.Settings.modifiers.bsml", this);
        }

        public void Dispose()
        {
            GameplaySetup.instance.RemoveTab("Chroma");
        }

#pragma warning disable CA1822
        [UsedImplicitly]
        [UIValue("rgbevents")]
#pragma warning disable SA1201
        public bool ChromaEventsDisabled
#pragma warning restore SA1201
        {
            get => _config.ChromaEventsDisabled;
            set => _config.ChromaEventsDisabled = value;
        }

        [UsedImplicitly]
        [UIValue("platform")]
        public bool EnvironmentEnhancementsDisabled
        {
            get => _config.EnvironmentEnhancementsDisabled;
            set
            {
                _beatmapDataCache.difficultyBeatmap = null;
                _config.EnvironmentEnhancementsDisabled = value;
            }
        }

        [UsedImplicitly]
        [UIValue("notecolors")]
        public bool NoteColoringDisabled
        {
            get => _config.NoteColoringDisabled;
            set => _config.NoteColoringDisabled = value;
        }

        [UsedImplicitly]
        [UIValue("zenwalls")]
        public bool ForceZenWallsEnabled
        {
            get => _config.ForceZenWallsEnabled;
            set
            {
                _beatmapDataCache.difficultyBeatmap = null;
                _config.ForceZenWallsEnabled = value;
            }
        }

        [UsedImplicitly]
        [UIValue("environmentenabled")]
        public bool CustomEnvironmentEnabled
        {
            get => _config.CustomEnvironmentEnabled;
            set
            {
                _beatmapDataCache.difficultyBeatmap = null;
                _config.CustomEnvironmentEnabled = value;
            }
        }

        [UsedImplicitly]
        [UIValue("environment")]
        public string? CustomEnvironment
        {
            get
            {
                string? name = _config.CustomEnvironment;
                return name != null && _savedEnvironmentLoader.Environments.ContainsKey(name) ? name : null;
            }

            set
            {
                _beatmapDataCache.difficultyBeatmap = null;
                _config.CustomEnvironment = value;
            }
        }

        [UsedImplicitly]
        [UIAction("environmentformat")]
        private string FormatCustomEnvironment(string? name)
        {
            if (name == null)
            {
                return NO_ENVIRONMENT;
            }

            SavedEnvironment environment = _savedEnvironmentLoader.Environments[name]!;
            return $"{environment.Name} v{environment.EnvironmentVersion}";
        }
#pragma warning restore CA1822
    }
}
