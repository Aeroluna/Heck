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
        private readonly Config _config;

        [UsedImplicitly]
        [UIValue("environmentoptions")]
        private readonly List<object?> _environmentOptions;

        private ChromaSettingsUI(Config config)
        {
            _config = config;
            _environmentOptions = SavedEnvironmentLoader.Environments.Cast<object?>().ToList();

            // TODO: find some way to disable this for DynamicInit
            GameplaySetup.instance.AddTab("Chroma", "Chroma.Settings.modifiers.bsml", this);
        }

#pragma warning disable CA1822
        [UsedImplicitly]
        [UIValue("rgbevents")]
        public bool ChromaEventsDisabled
        {
            get => _config.ChromaEventsDisabled;
            set => _config.ChromaEventsDisabled = value;
        }

        [UsedImplicitly]
        [UIValue("platform")]
        public bool EnvironmentEnhancementsDisabled
        {
            get => _config.EnvironmentEnhancementsDisabled;
            set => _config.EnvironmentEnhancementsDisabled = value;
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
            set => _config.ForceZenWallsEnabled = value;
        }

        [UsedImplicitly]
        [UIValue("environmentenabled")]
        public bool CustomEnvironmentEnabled
        {
            get => _config.CustomEnvironmentEnabled;
            set => _config.CustomEnvironmentEnabled = value;
        }

        [UsedImplicitly]
        [UIValue("environment")]
        public SavedEnvironment? CustomEnvironment
        {
            get => _config.CustomEnvironment;
            set => _config.CustomEnvironment = value;
        }

        [UsedImplicitly]
        [UIAction("environmentformat")]
        private string FormatCustomEnvironment(SavedEnvironment? environment)
        {
            return environment == null ? "None" : $"{environment.Name} v{environment.EnvironmentVersion}";
        }
#pragma warning restore CA1822
    }
}
