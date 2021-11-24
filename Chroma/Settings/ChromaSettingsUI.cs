using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace Chroma.Settings
{
    internal class ChromaSettingsUI : PersistentSingleton<ChromaSettingsUI>
    {
#pragma warning disable CA1822
        // Events
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
#pragma warning restore CA1822
    }
}
