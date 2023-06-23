using System;
using BepInEx.Configuration;
using Chroma.Extras;
using Heck.SettingsSetter;
using static Chroma.ChromaController;
using Loader = SongCore.Loader;

// ReSharper disable MemberCanBeMadeStatic.Global
namespace Chroma.Settings
{
    internal class Config
    {
        private static Config? _instance;

        internal Config(ConfigFile configFile)
        {
            _instance = this;

            ChromaEventsDisabled = SettingSetterSettableSettingsManager.CreateSettableConfigEntry(configFile.Bind(
                ID,
                "Disable Chroma Events",
                false,
                "Disable everything Chroma, but you'll break my heart."));

            EnvironmentEnhancementsDisabled = SettingSetterSettableSettingsManager.CreateSettableConfigEntry(configFile.Bind(
                ID,
                "Disable Environment Enhancements",
                false,
                "Disable Chroma's ability to manage the environment."));

            NoteColoringDisabled = SettingSetterSettableSettingsManager.CreateSettableConfigEntry(configFile.Bind(
                ID,
                "Disable Note Coloring",
                false,
                "Blame Mawntee."));

            ForceZenWallsEnabled = SettingSetterSettableSettingsManager.CreateSettableConfigEntry(configFile.Bind(
                ID,
                "Force Zen Mode Walls",
                false,
                "Forces walls to be enabled in the Zen Mode modifier."));

            CustomEnvironmentEnabled = SettingSetterSettableSettingsManager.CreateSettableConfigEntry(configFile.Bind(
                ID,
                "Use Custom Environment",
                false,
                "Yay custom environments!!!"));

            CustomEnvironment = configFile.Bind<string?>(
                ID,
                "Custom Environment",
                null,
                "The name of the custom environment to use.");

            PrintEnvironmentEnhancementDebug = configFile.Bind(
                "Debug",
                "Print Environment Enhancement",
                false,
                "Print environment enhancement information to the log.");

            ChromaEventsDisabled.ConfigEntry.SettingChanged += (_, _) =>
            {
                ChromaUtils.SetSongCoreCapability(CAPABILITY, !ChromaEventsDisabled.Value);
                if (Loader.Instance != null)
                {
                    Loader.Instance.RefreshSongs();
                }
            };
        }

        internal static Config Instance => _instance ?? throw new InvalidOperationException("Chroma Config instance not yet created.");

        internal SettableConfigEntry<bool> ChromaEventsDisabled { get; }

        internal SettableConfigEntry<bool> EnvironmentEnhancementsDisabled { get; }

        internal SettableConfigEntry<bool> NoteColoringDisabled { get; }

        internal SettableConfigEntry<bool> ForceZenWallsEnabled { get; }

        internal SettableConfigEntry<bool> CustomEnvironmentEnabled { get; }

        internal ConfigEntry<string?> CustomEnvironment { get; }

        internal ConfigEntry<bool> PrintEnvironmentEnhancementDebug { get; }
    }
}
