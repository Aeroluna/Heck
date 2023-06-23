using BepInEx.Configuration;
using UnityEngine;

namespace Heck.Settings
{
    internal class Config
    {
        internal Config(ConfigFile configFile)
        {
            ReLoader = new ReLoaderSettings(configFile);
        }

        internal ReLoaderSettings ReLoader { get; }

        internal class ReLoaderSettings
        {
            private const string SECTION_NAME = "ReLoader";

            internal ReLoaderSettings(ConfigFile configFile)
            {
                Reload = configFile.Bind(
                    SECTION_NAME,
                    "Reload",
                    KeyCode.Space,
                    "KeyCode used to hot reload. In the menu, this will reload the currently selected difficulty and in-game this will reload the current playing song.");
                SaveTime = configFile.Bind(
                    SECTION_NAME,
                    "Save Time",
                    KeyCode.LeftControl,
                    "KeyCode used to save the current time in the song.");
                JumpToSavedTime = configFile.Bind(
                    SECTION_NAME,
                    "Jump To Saved Time",
                    KeyCode.Space,
                    "KeyCode used to jump to the saved time.");
                ScrubBackwards = configFile.Bind(
                    SECTION_NAME,
                    "Scrub Backwards",
                    KeyCode.LeftArrow,
                    "KeyCode used to scrub backwards in time.");
                ScrubForwards = configFile.Bind(
                    SECTION_NAME,
                    "Scrub Forwards",
                    KeyCode.RightArrow,
                    "KeyCode used to scrub forwards in time.");
                ScrubIncrement = configFile.Bind(
                    SECTION_NAME,
                    "Scrub Increment",
                    5f,
                    "How long in seconds to skip when scrubbing.");
                ReloadOnRestart = configFile.Bind(
                    SECTION_NAME,
                    "Reload On Restart",
                    false,
                    "Whether or not to hot reload the difficulty file when restarting.");
            }

            internal ConfigEntry<KeyCode> Reload { get; }

            internal ConfigEntry<KeyCode> SaveTime { get; }

            internal ConfigEntry<KeyCode> JumpToSavedTime { get; }

            internal ConfigEntry<KeyCode> ScrubBackwards { get; }

            internal ConfigEntry<KeyCode> ScrubForwards { get; }

            internal ConfigEntry<float> ScrubIncrement { get; }

            internal ConfigEntry<bool> ReloadOnRestart { get; }
        }
    }
}
