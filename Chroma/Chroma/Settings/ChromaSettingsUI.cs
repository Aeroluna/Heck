namespace Chroma.Settings
{
    using BeatSaberMarkupLanguage.Attributes;

    internal class ChromaSettingsUI : PersistentSingleton<ChromaSettingsUI>
    {
        // Events
        [UIValue("rgbevents")]
        public bool CustomColorEventsEnabled
        {
            get => !ChromaConfig.Instance!.CustomColorEventsEnabled;
            set => ChromaConfig.Instance!.CustomColorEventsEnabled = !value;
        }

        [UIValue("platform")]
        public bool EnvironmentEnhancementsEnabled
        {
            get => !ChromaConfig.Instance!.EnvironmentEnhancementsEnabled;
            set => ChromaConfig.Instance!.EnvironmentEnhancementsEnabled = !value;
        }

        [UIValue("zenwalls")]
        public bool ForceZenWallsEnabled
        {
            get => ChromaConfig.Instance!.ForceZenWallsEnabled;
            set => ChromaConfig.Instance!.ForceZenWallsEnabled = value;
        }
    }
}
