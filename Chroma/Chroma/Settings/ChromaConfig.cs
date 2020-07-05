namespace Chroma.Settings
{
    public class ChromaConfig
    {
        public static ChromaConfig Instance { get; set; }

        public bool LightshowModifier { get; set; } = false;

        public bool CustomColorEventsEnabled { get; set; } = true;

        public bool EnvironmentEnhancementsEnabled { get; set; } = true;

        // fun secret lightshow menu
        public bool LightshowMenu { get; set; } = false;

        public bool PlayersPlace { get; set; } = false;

        public bool Spectrograms { get; set; } = false;

        public bool BackColumns { get; set; } = false;

        public bool Buildings { get; set; } = false;
    }
}
