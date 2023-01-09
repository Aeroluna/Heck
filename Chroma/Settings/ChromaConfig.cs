using System;
using System.Linq;
using Chroma.EnvironmentEnhancement.Saved;
using Chroma.Extras;
using Heck.SettingsSetter;
using IPA.Config.Data;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using JetBrains.Annotations;
using static Chroma.ChromaController;
using static Chroma.Settings.ChromaSettableSettings;
using Loader = SongCore.Loader;

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

        public bool CustomEnvironmentEnabled
        {
            get => CustomEnvironmentEnabledSetting.Value;
            set => CustomEnvironmentEnabledSetting.Value = value;
        }

        [UseConverter(typeof(SavedEnvironmentConverter))]
        public SavedEnvironment? CustomEnvironment { get; set; }
#pragma warning restore CA1822

        [UsedImplicitly]
        public bool PrintEnvironmentEnhancementDebug { get; set; }

        public class SavedEnvironmentConverter : ValueConverter<SavedEnvironment>
        {
            public override Value ToValue(SavedEnvironment? obj, object parent)
            {
                return new Text(obj?.FileName ?? "null");
            }

            public override SavedEnvironment? FromValue(Value? value, object parent)
            {
                return SavedEnvironmentLoader.Environments.FirstOrDefault(n => n?.FileName == ((Text?)value)?.Value);
            }
        }
    }

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
}
