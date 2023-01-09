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
        [UsedImplicitly]
        [UIValue("environmentoptions")]
        private readonly List<object?> _environmentOptions;

        private ChromaSettingsUI()
        {
            _environmentOptions = SavedEnvironmentLoader.Environments.Cast<object?>().ToList();

            // TODO: find some way to disable this for DynamicInit
            GameplaySetup.instance.AddTab("Chroma", "Chroma.Settings.modifiers.bsml", this);
        }

#pragma warning disable CA1822
        [UsedImplicitly]
        [UIValue("rgbevents")]
        public bool ChromaEventsDisabled
        {
            get => ChromaConfig.Instance.ChromaEventsDisabled;
            set => ChromaConfig.Instance.ChromaEventsDisabled = value;
        }

        [UsedImplicitly]
        [UIValue("platform")]
        public bool EnvironmentEnhancementsDisabled
        {
            get => ChromaConfig.Instance.EnvironmentEnhancementsDisabled;
            set => ChromaConfig.Instance.EnvironmentEnhancementsDisabled = value;
        }

        [UsedImplicitly]
        [UIValue("notecolors")]
        public bool NoteColoringDisabled
        {
            get => ChromaConfig.Instance.NoteColoringDisabled;
            set => ChromaConfig.Instance.NoteColoringDisabled = value;
        }

        [UsedImplicitly]
        [UIValue("zenwalls")]
        public bool ForceZenWallsEnabled
        {
            get => ChromaConfig.Instance.ForceZenWallsEnabled;
            set => ChromaConfig.Instance.ForceZenWallsEnabled = value;
        }

        [UsedImplicitly]
        [UIValue("environmentenabled")]
        public bool CustomEnvironmentEnabled
        {
            get => ChromaConfig.Instance.CustomEnvironmentEnabled;
            set => ChromaConfig.Instance.CustomEnvironmentEnabled = value;
        }

        [UsedImplicitly]
        [UIValue("environment")]
        public SavedEnvironment? CustomEnvironment
        {
            get => ChromaConfig.Instance.CustomEnvironment;
            set => ChromaConfig.Instance.CustomEnvironment = value;
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
