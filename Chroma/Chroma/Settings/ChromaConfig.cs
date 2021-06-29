namespace Chroma.Settings
{
    public class ChromaConfig
    {
        private static bool _customColorEventsEnabled = true;

        public static ChromaConfig? Instance { get; set; }

        public bool CustomColorEventsEnabled
        {
            get => _customColorEventsEnabled;
            set
            {
                SongCore.Loader.Instance?.RefreshSongs();
                Utils.ChromaUtils.SetSongCoreCapability(Plugin.REQUIREMENTNAME, value);
                _customColorEventsEnabled = value;
            }
        }

        public bool EnvironmentEnhancementsEnabled { get; set; } = true;

        public bool ForceZenWallsEnabled { get; set; } = false;

        public bool PrintEnvironmentEnhancementDebug { get; set; } = false;
    }
}
