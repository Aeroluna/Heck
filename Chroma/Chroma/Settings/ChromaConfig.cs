using Chroma.Events;
using Chroma.Utils;
using UnityEngine;
using static Chroma.ColourManager;

namespace Chroma.Settings
{
    internal static class ChromaConfig
    {
        internal enum LoadSettingsType
        {
            INITIAL,
            MANUAL,
            MENU_LOADED,
        }

        private static BS_Utils.Utilities.Config _iniProfile;

        /// <summary>
        /// Returns the player selected ini file for preferences
        /// </summary>
        internal static BS_Utils.Utilities.Config IniProfile
        {
            get
            {
                if (_iniProfile == null)
                {
                    string iniName = "default";
                    _iniProfile = new BS_Utils.Utilities.Config("Chroma/Preferences/" + iniName);
                }
                return _iniProfile;
            }
            set
            {
                _iniProfile = value;
            }
        }

        internal static int TimesLaunched { get; private set; } = 0;

        /// <summary>
        /// Enables debug features.  Significant performance cost.
        /// </summary>
        internal static bool DebugMode
        {
            get { return debugMode; }
            set
            {
                debugMode = value;
                SetBool("Other", "debugMode", debugMode);
            }
        }

        private static bool debugMode = false;

        internal static string Username { get; private set; } = "Unknown";
        internal static ulong UserID { get; private set; } = 0;

        #region Settings

        internal static SidePanelEnum SidePanel
        {
            get { return sidePanel; }
            set
            {
                sidePanel = value;
                SetFloat("Other", "sidePanel", (float)sidePanel);
            }
        }

        private static SidePanelEnum sidePanel = SidePanelEnum.DEFAULT;

        internal enum SidePanelEnum
        {
            DEFAULT = 0,
            CHROMA = 1,
            CHROMAWAIVER = 2
        }

        /// <summary>
        /// Required for any features that may cause dizziness, disorientation, nausea, seizures, or other forms of discomfort.
        /// </summary>
        internal static bool WaiverRead
        {
            get { return waiverRead; }
            set
            {
                if (value)
                {
                    waiverRead = true;
                    SetInt("Other", "safetyWaiver", 51228);
                }
            }
        }

        private static bool waiverRead = false;

        #endregion Settings

        #region Aesthetics

        internal static NamedColor NeonSignTop
        {
            get { return neonSignTop; }
            set
            {
                neonSignTop = value;
                SetString("Aesthetics", "signColourB", value.name);
            }
        }

        private static NamedColor neonSignTop = ChromaSettingsUI.colourPresets[0];

        internal static NamedColor NeonSignBottom
        {
            get { return neonSignBottom; }
            set
            {
                neonSignBottom = value;
                SetString("Aesthetics", "signColourA", value.name);
            }
        }

        private static NamedColor neonSignBottom = ChromaSettingsUI.colourPresets[0];

        internal static NamedColor LaserPointer
        {
            get { return laserPointer; }
            set
            {
                laserPointer = value;
                SetString("Aesthetics", "laserPointerColour", value.name);
            }
        }

        private static NamedColor laserPointer = ChromaSettingsUI.colourPresets[0];

        internal static NamedColor PlatformAccoutrements
        {
            get { return platformAccoutrements; }
            set
            {
                platformAccoutrements = value;
                SetString("Aesthetics", "platformAccoutrements", value.name);
            }
        }

        private static NamedColor platformAccoutrements = ChromaSettingsUI.colourPresets[0];

        #endregion Aesthetics

        #region Events

        internal static bool LightshowModifier
        {
            get { return lightshowModifier; }
            set
            {
                lightshowModifier = value;
                SetBool("Modifiers", "lightshowModifier", lightshowModifier);
            }
        }

        private static bool lightshowModifier;

        internal static bool CustomColourEventsEnabled
        {
            get { return customColourEventsEnabled; }
            set
            {
                customColourEventsEnabled = value;
                SetBool("Map", "customColourEventsEnabled", customColourEventsEnabled);
            }
        }

        private static bool customColourEventsEnabled = true;

        internal static bool NoteColourEventsEnabled
        {
            get { return noteColourEventsEnabled; }
            set
            {
                noteColourEventsEnabled = value;
                SetBool("Map", "noteColourEventsEnabled", noteColourEventsEnabled);
            }
        }

        private static bool noteColourEventsEnabled = true;

        internal static bool EnvironmentEnhancementsEnabled
        {
            get { return environmentEnhancementsEnabled; }
            set
            {
                environmentEnhancementsEnabled = value;
                SetBool("Map", "environmentEnhancementsEnabled", environmentEnhancementsEnabled);
            }
        }

        private static bool environmentEnhancementsEnabled = true;

        #endregion Events

        #region Technicolour

        internal static bool TechnicolourEnabled
        {
            get { return technicolourEnabled; }
            set
            {
                technicolourEnabled = value;
                SetBool("Technicolour", "technicolourEnabled", technicolourEnabled);
            }
        }

        private static bool technicolourEnabled = false;

        internal static TechnicolourStyle TechnicolourLightsStyle
        {
            get
            {
                return technicolourLightsStyle;
            }
            set
            {
                technicolourLightsStyle = value;
                SetInt("Technicolour", "technicolourLightsStyle", (int)technicolourLightsStyle);
            }
        }

        private static TechnicolourStyle technicolourLightsStyle = TechnicolourStyle.OFF;

        internal static TechnicolourLightsGrouping TechnicolourLightsGrouping
        {
            get { return technicolourLightsGrouping; }
            set
            {
                technicolourLightsGrouping = value;
                SetFloat("Technicolour", "technicolourLightsGrouping", (int)technicolourLightsGrouping);
            }
        }

        private static TechnicolourLightsGrouping technicolourLightsGrouping = TechnicolourLightsGrouping.STANDARD;

        internal static float TechnicolourLightsFrequency
        {
            get { return technicolourLightsFrequency; }
            set
            {
                technicolourLightsFrequency = value;
                SetFloat("Technicolour", "technicolourLightsFrequency", technicolourLightsFrequency);
            }
        }

        private static float technicolourLightsFrequency = 0.1f;

        internal static TechnicolourStyle TechnicolourSabersStyle
        {
            get
            {
                return technicolourSabersStyle;
            }
            set
            {
                technicolourSabersStyle = value;
                SetInt("Technicolour", "technicolourSabersStyle", (int)technicolourSabersStyle);
            }
        }

        private static TechnicolourStyle technicolourSabersStyle = TechnicolourStyle.OFF;

        internal static TechnicolourStyle TechnicolourBlocksStyle
        {
            get
            {
                return technicolourBlocksStyle;
            }
            set
            {
                technicolourBlocksStyle = value;
                SetInt("Technicolour", "technicolourBlocksStyle", (int)technicolourBlocksStyle);
            }
        }

        private static TechnicolourStyle technicolourBlocksStyle = TechnicolourStyle.OFF;

        internal static TechnicolourStyle TechnicolourWallsStyle
        {
            get
            {
                return technicolourWallsStyle;
            }
            set
            {
                technicolourWallsStyle = value;
                SetInt("Technicolour", "technicolourWallsStyle", (int)technicolourWallsStyle);
            }
        }

        private static TechnicolourStyle technicolourWallsStyle = TechnicolourStyle.OFF;

        internal static TechnicolourStyle TechnicolourBombsStyle
        {
            get
            {
                return technicolourBombsStyle;
            }
            set
            {
                technicolourBombsStyle = value;
                SetInt("Technicolour", "technicolourBombsStyle", (int)technicolourBombsStyle);
            }
        }

        private static TechnicolourStyle technicolourBombsStyle = TechnicolourStyle.OFF;

        internal static bool MatchTechnicolourSabers
        {
            get { return matchTechnicolourSabers; }
            set
            {
                matchTechnicolourSabers = value;
                SetBool("Map", "matchTechnicolourSabers", matchTechnicolourSabers);
            }
        }

        private static bool matchTechnicolourSabers = true;

        #endregion Technicolour

        #region Lightshow

        /// Secret stuffs
        ///
        internal static bool LightshowMenu
        {
            get { return lightshowMenu; }
            set
            {
                if (value)
                {
                    lightshowMenu = true;
                    SetInt("Lightshow", "lightshowMenu", 6777);
                }
            }
        }

        private static bool lightshowMenu = false;

        internal static bool PlayersPlace
        {
            get { return playersPlace; }
            set
            {
                playersPlace = value;
                SetBool("Lightshow", "playersPlace", value);
            }
        }

        private static bool playersPlace = false;

        internal static bool Spectrograms
        {
            get { return spectrograms; }
            set
            {
                spectrograms = value;
                SetBool("Lightshow", "spectrograms", value);
            }
        }

        private static bool spectrograms = false;

        internal static bool BackColumns
        {
            get { return backColumns; }
            set
            {
                backColumns = value;
                SetBool("Lightshow", "backColumns", value);
            }
        }

        private static bool backColumns = false;

        internal static bool Buildings
        {
            get { return buildings; }
            set
            {
                buildings = value;
                SetBool("Lightshow", "buildings", value);
            }
        }

        private static bool buildings = false;

        #endregion Lightshow

        internal static void Init()
        {
            Plugin.MainMenuLoadedEvent += OnMainMenuLoaded;
            Plugin.SongSceneLoadedEvent += OnSongLoaded;

            Plugin.MainMenuLoadedEvent += CleanupSongEvents;
            Plugin.SongSceneLoadedEvent += CleanupSongEvents;
        }

        private static void OnMainMenuLoaded()
        {
            RemoveNoteTypeColourOverride(NoteType.NoteA);
            RemoveNoteTypeColourOverride(NoteType.NoteB);

            LoadSettings(LoadSettingsType.MENU_LOADED);
        }

        private static void OnSongLoaded()
        {
            RemoveNoteTypeColourOverride(NoteType.NoteA);
            RemoveNoteTypeColourOverride(NoteType.NoteB);

            RefreshLights();
        }

        private static void CleanupSongEvents()
        {
            ChromaObstacleColourEvent.ObstacleColours.Clear();
            ChromaNoteColourEvent.NoteColours.Clear();
            ChromaNoteColourEvent.SavedNoteColours.Clear();
            ChromaBombColourEvent.BombColours.Clear();
            ChromaLightColourEvent.LightColours.Clear();
            ChromaGradientEvent.Gradients.Clear();

            HarmonyPatches.ColorNoteVisualsHandleNoteControllerDidInitEvent.noteColoursActive = false;
            HarmonyPatches.ObstacleControllerInit.ClearObstacleColors();

            Extensions.SaberColourizer.currentAColor = null;
            Extensions.SaberColourizer.currentBColor = null;

            ChromaGradientEvent.Clear();
            VFX.TechnicolourController.Clear();

            ClearLightSwitches();

            VFX.MayhemEvent.ClearManager();
        }

        internal static void LoadSettings(LoadSettingsType type)
        {
            Logger.Log("Loading settings [" + type.ToString() + "]", Logger.Level.INFO);

            string iniName = "settings";
            IniProfile = new BS_Utils.Utilities.Config("Chroma/Preferences/" + iniName);

            BS_Utils.Gameplay.GetUserInfo.UpdateUserInfo();
            Username = BS_Utils.Gameplay.GetUserInfo.GetUserName();
            UserID = BS_Utils.Gameplay.GetUserInfo.GetUserID();

            if (type == LoadSettingsType.INITIAL) SetInt("Other", "timesLaunched", GetInt("Other", "timesLaunched", 0) + 1);

            /*
            * MAP
            */

            customColourEventsEnabled = GetBool("Map", "customColourEventsEnabled", true);
            noteColourEventsEnabled = GetBool("Map", "noteColourEventsEnabled", true);
            environmentEnhancementsEnabled = GetBool("Map", "environmentEnhancementsEnabled", true);
            ChromaUtils.SetSongCoreCapability(Plugin.REQUIREMENT_NAME, CustomColourEventsEnabled);

            /*
            * TECHNICOLOUR
            */

            technicolourEnabled = GetBool("Technicolour", "technicolourEnabled", false);

            technicolourLightsStyle = (TechnicolourStyle)GetInt("Technicolour", "technicolourLightsStyle", 1);
            technicolourLightsGrouping = (TechnicolourLightsGrouping)GetInt("Technicolour", "technicolourLightsGrouping", 1);
            technicolourLightsFrequency = GetFloat("Technicolour", "technicolourLightsFrequency", technicolourLightsFrequency);
            technicolourSabersStyle = (TechnicolourStyle)GetInt("Technicolour", "technicolourSabersStyle", 0);
            technicolourBlocksStyle = (TechnicolourStyle)GetInt("Technicolour", "technicolourBlocksStyle", 0);
            technicolourWallsStyle = (TechnicolourStyle)GetInt("Technicolour", "technicolourWallsStyle", 0);
            technicolourBombsStyle = (TechnicolourStyle)GetInt("Technicolour", "technicolourBombsStyle", 0);
            matchTechnicolourSabers = GetBool("Technicolour", "matchTechnicolourSabers", false);

            TechnicolourWarmPalette = new Color[4] { new Color(1, 0, 0), new Color(1, 0, 1), new Color(1, 0.6f, 0), new Color(1, 0, 0.4f) };
            TechnicolourColdPalette = new Color[4] { new Color(0, 0.501f, 1), new Color(0, 1, 0), new Color(0, 0, 1), new Color(0, 1, 0.8f) };

            /*
            * AESTHETICS
            */

            LaserPointer = ChromaSettingsUI.StringToColour(GetString("Aesthetics", "laserPointerColour", "DEFAULT"));
            NeonSignBottom = ChromaSettingsUI.StringToColour(GetString("Aesthetics", "signColourA", "DEFAULT"));
            NeonSignTop = ChromaSettingsUI.StringToColour(GetString("Aesthetics", "signColourB", "DEFAULT"));
            PlatformAccoutrements = ChromaSettingsUI.StringToColour(GetString("Aesthetics", "platformAccoutrements", "DEFAULT"));

            /*
            * MODIFIERS
            */

            lightshowModifier = GetBool("Modifiers", "lightshowModifier", false);

            /*
            * OTHER
            */

            sidePanel = (SidePanelEnum)GetFloat("Other", "sidePanel", 0);

            debugMode = GetBool("Other", "debugMode", false);

            waiverRead = GetInt("Other", "safetyWaiver", 0) == 51228;

            /*
            * LIGHTSHOW
            */

            lightshowMenu = GetInt("Lightshow", "lightshowMenu", 0, false) == 6777;
            playersPlace = GetBool("Lightshow", "playersPlace", false, false);
            spectrograms = GetBool("Lightshow", "spectrograms", false, false);
            backColumns = GetBool("Lightshow", "backColumns", false, false);
            buildings = GetBool("Lightshow", "buildings", false, false);

            RefreshLights();
        }

        #region configshortcuts

        internal static void SetBool(string category, string name, bool value)
        {
            IniProfile.SetBool(category, name, value);
        }

        internal static void SetFloat(string category, string name, float value)
        {
            IniProfile.SetFloat(category, name, value);
        }

        internal static void SetInt(string category, string name, int value)
        {
            IniProfile.SetInt(category, name, value);
        }

        internal static void SetString(string category, string name, string value)
        {
            IniProfile.SetString(category, name, value);
        }

        internal static bool GetBool(string category, string name, bool def = false, bool autoSave = true)
        {
            return IniProfile.GetBool(category, name, def, autoSave);
        }

        internal static float GetFloat(string category, string name, float def, bool autoSave = true)
        {
            return IniProfile.GetFloat(category, name, def, autoSave);
        }

        internal static int GetInt(string category, string name, int def, bool autoSave = true)
        {
            return IniProfile.GetInt(category, name, def, autoSave);
        }

        internal static string GetString(string category, string name, string def, bool autoSave = true)
        {
            return IniProfile.GetString(category, name, def, autoSave);
        }

        #endregion configshortcuts
    }
}