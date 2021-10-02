namespace Chroma.Settings
{
    using Heck.SettingsSetter;
    using static ChromaSettableSettings;

    public class ChromaConfig
    {
        private static ChromaConfig? _instance;

        public static ChromaConfig Instance
        {
            get => _instance ?? throw new System.InvalidOperationException("ChromaConfig instance not yet created.");
            set => _instance = value;
        }

        public bool ChromaEventsDisabled
        {
            get => ChromaEventsDisabledSetting.Value;
            set
            {
                ChromaEventsDisabledSetting.Value = value;
                Utils.ChromaUtils.SetSongCoreCapability(Plugin.REQUIREMENTNAME, !ChromaEventsDisabledSetting.Value);
                SongCore.Loader.Instance?.RefreshSongs();
            }
        }

        public bool EnvironmentEnhancementsDisabled
        {
            get => EnvironmentEnhancementsDisabledSetting.Value;
            set => EnvironmentEnhancementsDisabledSetting.Value = value;
        }

        public bool ForceZenWallsEnabled
        {
            get => ForceZenWallsEnabledSetting.Value;
            set => ForceZenWallsEnabledSetting.Value = value;
        }

        public bool PrintEnvironmentEnhancementDebug { get; set; } = false;
    }

    internal static class ChromaSettableSettings
    {
        internal static SettableSetting<bool> ChromaEventsDisabledSetting { get; } = new SettableSetting<bool>("Chroma", "Disable Chroma Events");

        internal static SettableSetting<bool> EnvironmentEnhancementsDisabledSetting { get; } = new SettableSetting<bool>("Chroma", "Disable Environment Enhancements");

        internal static SettableSetting<bool> ForceZenWallsEnabledSetting { get; } = new SettableSetting<bool>("Chroma", "Force Zen Mode Walls");

        internal static void SetupSettableSettings()
        {
            SettingSetterSettableSettingsManager.RegisterSettableSetting("_chroma", "_disableChromaEvents", ChromaEventsDisabledSetting);
            SettingSetterSettableSettingsManager.RegisterSettableSetting("_chroma", "_disableEnvironmentEnhancements", EnvironmentEnhancementsDisabledSetting);
            SettingSetterSettableSettingsManager.RegisterSettableSetting("_chroma", "_forceZenModeWalls", ForceZenWallsEnabledSetting);
        }
    }
}
