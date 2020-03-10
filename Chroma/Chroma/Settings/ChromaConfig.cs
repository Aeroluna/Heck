using Chroma.Events;
using Chroma.Utils;
using System;
using System.Linq;
using UnityEngine;
using static Chroma.ColourManager;

namespace Chroma.Settings
{
    public static class ChromaConfig
    {
        public enum LoadSettingsType
        {
            INITIAL,
            MANUAL,
            MENU_LOADED,
        }

        private static BS_Utils.Utilities.Config _iniProfile;

        /// <summary>
        /// Returns the player selected ini file for preferences
        /// </summary>
        public static BS_Utils.Utilities.Config IniProfile
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

        public static int TimesLaunched { get; private set; } = 0;

        public static MainSettingsModelSO MainSettingsModel { get; private set; }

        public static bool oldHaptics = true;

        /// <summary>
        /// Enables debug features.  Significant performance cost.
        /// </summary>
        public static bool DebugMode
        {
            get { return debugMode; }
            set
            {
                debugMode = value;
                SetBool("Other", "debugMode", debugMode);
            }
        }

        private static bool debugMode = false;

        public static string Username { get; private set; } = "Unknown";
        public static ulong UserID { get; private set; } = 0;

        public static ChromaSettingsUI.SidePanelEnum SidePanel
        {
            get { return sidePanel; }
            set
            {
                sidePanel = value;
                SetFloat("Other", "sidePanel", (float)sidePanel);
            }
        }

        private static ChromaSettingsUI.SidePanelEnum sidePanel = ChromaSettingsUI.SidePanelEnum.Default;

        /// <summary>
        /// Enables checking for tailored maps.
        /// This will not disable map checking entirely, it will simply prevent a map from being detected as created for a specific gamemode.
        /// </summary>
        public static bool CustomMapCheckingEnabled
        {
            get { return customMapCheckingEnabled; }
            set
            {
                customMapCheckingEnabled = value;
                SetBool("Map", "customMapCheckingEnabled", customMapCheckingEnabled);
            }
        }

        private static bool customMapCheckingEnabled = true;

        public static bool CustomColourEventsEnabled
        {
            get { return customColourEventsEnabled; }
            set
            {
                customColourEventsEnabled = value;
                SetBool("Map", "customColourEventsEnabled", customColourEventsEnabled);
            }
        }

        private static bool customColourEventsEnabled = true;

        public static bool NoteColourEventsEnabled
        {
            get { return noteColourEventsEnabled; }
            set
            {
                noteColourEventsEnabled = value;
                SetBool("Map", "noteColourEventsEnabled", noteColourEventsEnabled);
            }
        }

        private static bool noteColourEventsEnabled = true;

        /// <summary>
        /// Global multiplier for audio sources used by Chroma
        /// </summary>
        public static float MasterVolume
        {
            get { return masterVolume; }
            set
            {
                masterVolume = value;
                SetFloat("Audio", "masterVolume", masterVolume);
            }
        }

        private static float masterVolume = 1f;

        /// <summary>
        /// Global multiplier for audio sources used by Chroma
        /// </summary>
        public static float SaberTrailStrength
        {
            get { return saberTrailStrength; }
            set
            {
                saberTrailStrength = value;
                SetFloat("Aesthetics", "saberTrailStrength", saberTrailStrength);
            }
        }

        private static float saberTrailStrength = 1f;

        /// <summary>
        /// Required for any features that may cause dizziness, disorientation, nausea, seizures, or other forms of discomfort.
        /// </summary>
        public static bool WaiverRead
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

        #region modifiers

        public static bool LightshowModifier
        {
            get { return lightshowModifier; }
            set
            {
                lightshowModifier = value;
                SetBool("Modifiers", "lightshowModifier", lightshowModifier);
            }
        }

        private static bool lightshowModifier;

        #endregion modifiers

        #region technicolour

        public static bool TechnicolourEnabled
        {
            get { return technicolourEnabled; }
            set
            {
                technicolourEnabled = value;
                SetBool("Technicolour", "technicolourEnabled", technicolourEnabled);
            }
        }

        private static bool technicolourEnabled = false;

        public static TechnicolourStyle TechnicolourLightsStyle
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

        public static TechnicolourLightsGrouping TechnicolourLightsGrouping
        {
            get { return technicolourLightsGrouping; }
            set
            {
                technicolourLightsGrouping = value;
                SetFloat("Technicolour", "technicolourLightsGrouping", (int)technicolourLightsGrouping);
            }
        }

        private static TechnicolourLightsGrouping technicolourLightsGrouping = TechnicolourLightsGrouping.STANDARD;

        public static float TechnicolourLightsFrequency
        {
            get { return technicolourLightsFrequency; }
            set
            {
                technicolourLightsFrequency = value;
                SetFloat("Technicolour", "technicolourLightsFrequency", technicolourLightsFrequency);
            }
        }

        private static float technicolourLightsFrequency = 0.1f;

        public static TechnicolourStyle TechnicolourSabersStyle
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

        public static TechnicolourStyle TechnicolourBlocksStyle
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

        public static TechnicolourStyle TechnicolourWallsStyle
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

        public static TechnicolourStyle TechnicolourBombsStyle
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

        public static bool MatchTechnicolourSabers
        {
            get { return matchTechnicolourSabers; }
            set
            {
                matchTechnicolourSabers = value;
                SetBool("Map", "matchTechnicolourSabers", matchTechnicolourSabers);
            }
        }

        private static bool matchTechnicolourSabers = true;

        #endregion technicolour

        #region lightshow

        /// Secret stuffs
        ///
        public static bool LightshowMenu
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

        public static bool PlayersPlace
        {
            get { return playersPlace; }
            set
            {
                playersPlace = value;
                SetBool("Lightshow", "playersPlace", value);
            }
        }

        private static bool playersPlace = false;

        public static bool Spectrograms
        {
            get { return spectrograms; }
            set
            {
                spectrograms = value;
                SetBool("Lightshow", "spectrograms", value);
            }
        }

        private static bool spectrograms = false;

        public static bool BackColumns
        {
            get { return backColumns; }
            set
            {
                backColumns = value;
                SetBool("Lightshow", "backColumns", value);
            }
        }

        private static bool backColumns = false;

        public static bool Buildings
        {
            get { return buildings; }
            set
            {
                buildings = value;
                SetBool("Lightshow", "buildings", value);
            }
        }

        private static bool buildings = false;

        #endregion lightshow

        /// <summary>
        /// Called when Chroma reloads the config files.
        /// </summary>
        public static event LoadSettingsDelegate LoadSettingsEvent;

        public delegate void LoadSettingsDelegate(BS_Utils.Utilities.Config iniProfile, LoadSettingsType type);

        internal static void Init()
        {
            LoadSettingsEvent += OnLoadSettingsEvent;

            ChromaPlugin.MainMenuLoadedEvent += OnMainMenuLoaded;
            ChromaPlugin.SongSceneLoadedEvent += OnSongLoaded;

            ChromaPlugin.MainMenuLoadedEvent += CleanupSongEvents;
            ChromaPlugin.SongSceneLoadedEvent += CleanupSongEvents;
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
            ChromaObstacleColourEvent.CustomObstacleColours.Clear();
            ChromaNoteColourEvent.CustomNoteColours.Clear();
            ChromaNoteColourEvent.SavedNoteColours.Clear();
            ChromaBombColourEvent.CustomBombColours.Clear();
            ChromaLightColourEvent.CustomLightColours.Clear();
            ChromaGradientEvent.CustomGradients.Clear();

            HarmonyPatches.ColorNoteVisualsHandleNoteControllerDidInitEvent.noteColoursActive = false;

            Extensions.SaberColourizer.currentAColor = null;
            Extensions.SaberColourizer.currentBColor = null;

            ChromaGradientEvent.Clear();
            VFX.TechnicolourController.Clear();

            LightSwitchs = null;

            VFX.MayhemEvent.manager = null;
        }

        internal static void LoadSettings(LoadSettingsType type)
        {
            string iniName = "settings";
            IniProfile = new BS_Utils.Utilities.Config("Chroma/Preferences/" + iniName);

            LoadSettingsEvent?.Invoke(IniProfile, type);
        }

        private static void OnLoadSettingsEvent(BS_Utils.Utilities.Config iniProfile, LoadSettingsType type)
        {
            try
            {
                ChromaLogger.Log("Loading settings [" + type.ToString() + "]", ChromaLogger.Level.INFO);

                string iniName = "settings";
                IniProfile = new BS_Utils.Utilities.Config("Chroma/Preferences/" + iniName);

                ChromaLogger.Log("--- From file " + iniName);

                BS_Utils.Gameplay.GetUserInfo.UpdateUserInfo();
                Username = BS_Utils.Gameplay.GetUserInfo.GetUserName();
                UserID = BS_Utils.Gameplay.GetUserInfo.GetUserID();

                if (DebugMode) ChromaLogger.Log("=== YOUR ID : " + UserID.ToString());

                if (type == LoadSettingsType.INITIAL)
                {
                    TimesLaunched = GetInt("Other", "timesLaunched", 0) + 1;
                    SetInt("Other", "timesLaunched", TimesLaunched);
                }

                /*
                 * MAP
                 */

                customMapCheckingEnabled = GetBool("Map", "customMapCheckingEnabled", true);
                customColourEventsEnabled = GetBool("Map", "customColourEventsEnabled", true);
                noteColourEventsEnabled = GetBool("Map", "noteColourEventsEnabled", true);
                ChromaUtils.SetSongCoreCapability("Chroma Lighting Events", CustomColourEventsEnabled);

                /*
                 * AUDIO
                 */

                masterVolume = Mathf.Clamp01(GetFloat("Audio", "masterVolume", 1));

                AudioUtil.Instance.SetVolume(masterVolume);

                /*
                 * TECHNICOLOUR
                 */

                if (type == LoadSettingsType.INITIAL)
                {
                    technicolourEnabled = GetBool("Technicolour", "technicolourEnabled", false);

                    technicolourLightsStyle = (TechnicolourStyle)GetInt("Technicolour", "technicolourLightsStyle", 1);
                    technicolourLightsGrouping = (TechnicolourLightsGrouping)GetInt("Technicolour", "technicolourLightsGrouping", 1);
                    technicolourLightsFrequency = GetFloat("Technicolour", "technicolourLightsFrequency", technicolourLightsFrequency);
                    technicolourSabersStyle = (TechnicolourStyle)GetInt("Technicolour", "technicolourSabersStyle", 0);
                    technicolourBlocksStyle = (TechnicolourStyle)GetInt("Technicolour", "technicolourBlocksStyle", 0);
                    technicolourWallsStyle = (TechnicolourStyle)GetInt("Technicolour", "technicolourWallsStyle", 0);
                    technicolourBombsStyle = (TechnicolourStyle)GetInt("Technicolour", "technicolourBombsStyle", 0);
                    matchTechnicolourSabers = GetBool("Technicolour", "matchTechnicolourSabers", false);
                }

                string[] technicolourColdString = GetString("Technicolour", "technicolourB", "0;128;255;255-0;255;0;255-0;0;255;255-0;255;204;255").Split('-');
                string[] technicolourWarmString = GetString("Technicolour", "technicolourA", "255;0;0;255-255;0;255;255-255;153;0;255-255;0;102;255").Split('-');

                Color[] technicolourCold = new Color[technicolourColdString.Length];
                Color[] technicolourWarm = new Color[technicolourWarmString.Length];

                for (int i = 0; i < Mathf.Max(technicolourCold.Length, technicolourWarm.Length); i++)
                {
                    if (i < technicolourCold.Length)
                    {
                        technicolourCold[i] = ColourFromString(technicolourColdString[i]);
                    }
                    if (i < technicolourWarm.Length)
                    {
                        technicolourWarm[i] = ColourFromString(technicolourWarmString[i]);
                    }
                }

                TechnicolourWarmPalette = technicolourWarm;
                TechnicolourColdPalette = technicolourCold;

                /*
                 * NOTES
                 */

                //ColourManager.A = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Notes", "colourA", "DEFAULT"), ColourManager.DefaultA);
                //ColourManager.B = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Notes", "colourB", "DEFAULT"), ColourManager.DefaultB);
                AltA = ChromaSettingsUI.GetColor(GetString("Notes", "colourAltA", "Notes Magenta"), DefaultAltA);
                AltB = ChromaSettingsUI.GetColor(GetString("Notes", "colourAltB", "Notes Green"), DefaultAltB);
                NonColoured = ChromaSettingsUI.GetColor(GetString("Notes", "colourNonColoured", "Notes White"), DefaultNonColoured);
                DoubleHit = ChromaSettingsUI.GetColor(GetString("Notes", "colourDuochrome", "Notes Purple"), DefaultDoubleHit);
                Super = ChromaSettingsUI.GetColor(GetString("Notes", "colourSuper", "Notes Gold"), DefaultSuper);

                /*
                 * LIGHTS
                 */

                //ColourManager.LightAmbient = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Lights", "lightAmbient", "DEFAULT"), ColourManager.DefaultLightAmbient);
                //ColourManager.LightA = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Lights", "lightColourA", "DEFAULT"), ColourManager.DefaultLightA);
                //ColourManager.LightB = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Lights", "lightColourB", "DEFAULT"), ColourManager.DefaultLightB);
                LightAltA = ChromaSettingsUI.GetColor(GetString("Lights", "lightColourAltA", "Light Magenta"), DefaultLightAltA);
                LightAltB = ChromaSettingsUI.GetColor(GetString("Lights", "lightColourAltB", "Light Green"), DefaultLightAltB);
                LightWhite = ChromaSettingsUI.GetColor(GetString("Lights", "lightColourWhite", "Light White"), DefaultLightWhite);
                LightGrey = ChromaSettingsUI.GetColor(GetString("Lights", "lightColourGrey", "Light Grey"), DefaultLightGrey);

                /*
                 * AESTHETICS
                 */

                //ColourManager.BarrierColour = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Aesthetics", "barrierColour", "DEFAULT"), ColourManager.DefaultBarrierColour);
                LaserPointerColour = ChromaSettingsUI.GetColor(GetString("Aesthetics", "laserPointerColour", "DEFAULT"), null);
                SignA = ChromaSettingsUI.GetColor(GetString("Aesthetics", "signColourA", "DEFAULT"), null);
                SignB = ChromaSettingsUI.GetColor(GetString("Aesthetics", "signColourB", "DEFAULT"), null);
                Platform = ChromaSettingsUI.GetColor(GetString("Aesthetics", "platformAccoutrements", "DEFAULT"), null);

                saberTrailStrength = GetFloat("Aesthetics", "saberTrailStrength", 1f);

                /*
                 * MODIFIERS
                 */
                lightshowModifier = GetBool("Modifiers", "lightshowModifier", false);

                /*
                 * OTHER
                 */

                sidePanel = (ChromaSettingsUI.SidePanelEnum)GetFloat("Other", "sidePanel", 1);

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

                if (type == LoadSettingsType.MANUAL) AudioUtil.Instance.PlayOneShotSound("ConfigReload.wav");
            }
            catch (Exception e)
            {
                ChromaLogger.Log("Error loading Chroma configs!  Waduhek", ChromaLogger.Level.ERROR);
                ChromaLogger.Log(e);
            }
        }

        public static void LoadSettingsModel()
        {
            MainSettingsModel = Resources.FindObjectsOfTypeAll<MainSettingsModelSO>().FirstOrDefault();
            if (MainSettingsModel)
            {
                ChromaLogger.Log("Found settings model", ChromaLogger.Level.DEBUG);
                oldHaptics = MainSettingsModel.controllersRumbleEnabled;
            }
        }

        #region configshortcuts

        public static void SetBool(string category, string name, bool value)
        {
            IniProfile.SetBool(category, name, value);
        }

        public static void SetFloat(string category, string name, float value)
        {
            IniProfile.SetFloat(category, name, value);
        }

        public static void SetInt(string category, string name, int value)
        {
            IniProfile.SetInt(category, name, value);
        }

        public static void SetString(string category, string name, string value)
        {
            IniProfile.SetString(category, name, value);
        }

        public static bool GetBool(string category, string name, bool def = false, bool autoSave = true)
        {
            return IniProfile.GetBool(category, name, def, autoSave);
        }

        public static float GetFloat(string category, string name, float def, bool autoSave = true)
        {
            return IniProfile.GetFloat(category, name, def, autoSave);
        }

        public static int GetInt(string category, string name, int def, bool autoSave = true)
        {
            return IniProfile.GetInt(category, name, def, autoSave);
        }

        public static string GetString(string category, string name, string def, bool autoSave = true)
        {
            return IniProfile.GetString(category, name, def, autoSave);
        }

        #endregion configshortcuts
    }
}