using System;
using System.Runtime.CompilerServices;
using Chroma.Extras;
using Heck.SettingsSetter;
using IPA.Config.Stores;
using JetBrains.Annotations;
using static Chroma.ChromaController;
using static Chroma.Settings.ChromaSettableSettings;
using Loader = SongCore.Loader;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

// ReSharper disable MemberCanBeMadeStatic.Global
namespace Chroma.Settings
{
    internal static class ChromaSettableSettings
    {
        internal static SettableSetting<bool> ChromaEventsDisabledSetting { get; } = new("Chroma", "Disable Chroma Events");

        internal static SettableSetting<bool> EnvironmentEnhancementsDisabledSetting { get; } = new("Chroma", "Disable Environment Enhancements");

        internal static SettableSetting<bool> NoteColoringDisabledSetting { get; } = new("Chroma", "Disable Note Coloring");

        internal static SettableSetting<bool> ForceZenWallsEnabledSetting { get; } = new("Chroma", "Force Zen Mode Walls");

        internal static SettableSetting<bool> CustomEnvironmentEnabledSetting { get; } = new("Chroma", "Use Custom Environment");

        internal static void SetupSettableSettings()
        {
            SettingSetterSettableSettingsManager.RegisterSettableSetting("_chroma", "_disableChromaEvents", ChromaEventsDisabledSetting);
            SettingSetterSettableSettingsManager.RegisterSettableSetting("_chroma", "_disableEnvironmentEnhancements", EnvironmentEnhancementsDisabledSetting);
            SettingSetterSettableSettingsManager.RegisterSettableSetting("_chroma", "_disableNoteColoring", NoteColoringDisabledSetting);
            SettingSetterSettableSettingsManager.RegisterSettableSetting("_chroma", "_forceZenModeWalls", ForceZenWallsEnabledSetting);
            SettingSetterSettableSettingsManager.RegisterSettableSetting("_chroma", "_useCustomEnvironment", CustomEnvironmentEnabledSetting);
        }
    }

    internal class Config
    {
        private static Config? _instance;

        public Config()
        {
            _instance = this;
        }

        public static Config Instance => _instance ?? throw new InvalidOperationException("Chroma Config instance not yet created.");

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

        public bool CustomEnvironmentEnabled
        {
            get => CustomEnvironmentEnabledSetting.Value;
            set => CustomEnvironmentEnabledSetting.Value = value;
        }

        public string? CustomEnvironment { get; set; }
#pragma warning restore CA1822

        [UsedImplicitly]
        public bool PrintEnvironmentEnhancementDebug { get; set; }
    }
}
