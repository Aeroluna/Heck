namespace Chroma.Settings
{
    using BeatSaberMarkupLanguage.Attributes;

    internal class ChromaSettingsUI : PersistentSingleton<ChromaSettingsUI>
    {
        // Events
        [UIValue("rgbevents")]
        public bool ChromaEventsDisabled
        {
            get => ChromaConfig.Instance.ChromaEventsDisabled;
            set => ChromaConfig.Instance.ChromaEventsDisabled = value;
        }

        [UIValue("platform")]
        public bool EnvironmentEnhancementsDisabled
        {
            get => ChromaConfig.Instance.EnvironmentEnhancementsDisabled;
            set => ChromaConfig.Instance.EnvironmentEnhancementsDisabled = value;
        }

        [UIValue("notecolors")]
        public bool NoteColoringDisabled
        {
            get => ChromaConfig.Instance.NoteColoringDisabled;
            set => ChromaConfig.Instance.NoteColoringDisabled = value;
        }

        [UIValue("zenwalls")]
        public bool ForceZenWallsEnabled
        {
            get => ChromaConfig.Instance.ForceZenWallsEnabled;
            set => ChromaConfig.Instance.ForceZenWallsEnabled = value;
        }
    }
}
