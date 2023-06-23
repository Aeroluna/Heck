using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using Chroma.EnvironmentEnhancement.Saved;
using JetBrains.Annotations;

namespace Chroma.Settings
{
    internal class ChromaSettingsUI
    {
        private const string NO_ENVIRONMENT = "None";

        private readonly Config _config;
        private readonly SavedEnvironmentLoader _savedEnvironmentLoader;

        [UsedImplicitly]
        [UIValue("environmentoptions")]
        private List<object?> _environmentOptions;

        private ChromaSettingsUI(Config config, SavedEnvironmentLoader savedEnvironmentLoader)
        {
            _config = config;
            _savedEnvironmentLoader = savedEnvironmentLoader;
            _environmentOptions = _savedEnvironmentLoader.Environments.Keys.Cast<object?>().Prepend(null).ToList();

            // TODO: find some way to disable this for DynamicInit
            GameplaySetup.instance.AddTab("Chroma", "Chroma.Settings.modifiers.bsml", this);
        }

#pragma warning disable CA1822
        [UsedImplicitly]
        [UIValue("rgbevents")]
        public bool ChromaEventsDisabled
        {
            get => _config.ChromaEventsDisabled;
            set => _config.ChromaEventsDisabled.Value = value;
        }

        [UsedImplicitly]
        [UIValue("platform")]
        public bool EnvironmentEnhancementsDisabled
        {
            get => _config.EnvironmentEnhancementsDisabled;
            set => _config.EnvironmentEnhancementsDisabled.Value = value;
        }

        [UsedImplicitly]
        [UIValue("notecolors")]
        public bool NoteColoringDisabled
        {
            get => _config.NoteColoringDisabled;
            set => _config.NoteColoringDisabled.Value = value;
        }

        [UsedImplicitly]
        [UIValue("zenwalls")]
        public bool ForceZenWallsEnabled
        {
            get => _config.ForceZenWallsEnabled;
            set => _config.ForceZenWallsEnabled.Value = value;
        }

        [UsedImplicitly]
        [UIValue("environmentenabled")]
        public bool CustomEnvironmentEnabled
        {
            get => _config.CustomEnvironmentEnabled;
            set => _config.CustomEnvironmentEnabled.Value = value;
        }

        [UsedImplicitly]
        [UIValue("environment")]
        public string? CustomEnvironment
        {
            get
            {
                string? name = _config.CustomEnvironment.Value;
                return name != null && _savedEnvironmentLoader.Environments.ContainsKey(name) ? name : null;
            }
            set => _config.CustomEnvironment.Value = value;
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
