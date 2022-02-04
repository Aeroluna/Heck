using System;
using Chroma.Extras;
using Heck.SettingsSetter;
using JetBrains.Annotations;
using SongCore;
using static Chroma.ChromaController;
using static Chroma.Settings.ChromaSettableSettings;

// ReSharper disable MemberCanBeMadeStatic.Global
namespace Chroma.Settings
{
    public class ChromaConfig
    {
        private static ChromaConfig? _instance;

        public static ChromaConfig Instance
        {
            get => _instance ?? throw new InvalidOperationException("ChromaConfig instance not yet created.");
            set => _instance = value;
        }

#pragma warning disable CA1822
        public bool ChromaEventsDisabled
        {
            get => ChromaEventsDisabledSetting.Value;
            set
            {
                ChromaEventsDisabledSetting.Value = value;
                ChromaUtils.SetSongCoreCapability(CAPABILITY, !ChromaEventsDisabledSetting.Value);
                if (Loader.Instance != null)
                {
                    Loader.Instance.RefreshSongs();
                }
            }
        }

        public bool EnvironmentEnhancementsDisabled
        {
            get => EnvironmentEnhancementsDisabledSetting.Value;
            set => EnvironmentEnhancementsDisabledSetting.Value = value;
        }

        public bool NoteColoringDisabled
        {
            get => NoteColoringDisabledSetting.Value;
            set => NoteColoringDisabledSetting.Value = value;
        }

        public bool ForceZenWallsEnabled
        {
            get => ForceZenWallsEnabledSetting.Value;
            set => ForceZenWallsEnabledSetting.Value = value;
        }
#pragma warning restore CA1822

        [UsedImplicitly]
        public bool PrintEnvironmentEnhancementDebug { get; set; }
    }

    internal static class ChromaSettableSettings
    {
        internal static SettableSetting<bool> ChromaEventsDisabledSetting { get; } = new("Chroma", "Disable Chroma Events");

        internal static SettableSetting<bool> EnvironmentEnhancementsDisabledSetting { get; } = new("Chroma", "Disable Environment Enhancements");

        internal static SettableSetting<bool> NoteColoringDisabledSetting { get; } = new("Chroma", "Disable Note Coloring");

        internal static SettableSetting<bool> ForceZenWallsEnabledSetting { get; } = new("Chroma", "Force Zen Mode Walls");

        internal static void SetupSettableSettings()
        {
            SettingSetterSettableSettingsManager.RegisterSettableSetting("_chroma", "_disableChromaEvents", ChromaEventsDisabledSetting);
            SettingSetterSettableSettingsManager.RegisterSettableSetting("_chroma", "_disableEnvironmentEnhancements", EnvironmentEnhancementsDisabledSetting);
            SettingSetterSettableSettingsManager.RegisterSettableSetting("_chroma", "_disableNoteColoring", NoteColoringDisabledSetting);
            SettingSetterSettableSettingsManager.RegisterSettableSetting("_chroma", "_forceZenModeWalls", ForceZenWallsEnabledSetting);
        }
    }
}
