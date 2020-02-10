using Chroma.Beatmap.Events.Legacy;
using Chroma.Misc;
using Chroma.Utils;
using IPA.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Chroma.Beatmap.Events;
using static Chroma.ColourManager;

namespace Chroma.Settings {

    public static class ChromaConfig {

        public enum LoadSettingsType {
            INITIAL,
            MANUAL,
            MENU_LOADED,
        }


        private static BS_Utils.Utilities.Config _iniProfile;

        /// <summary>
        /// Returns the player selected ini file for preferences
        /// </summary>
        public static BS_Utils.Utilities.Config IniProfile {
            get {
                if (_iniProfile == null) {
                    string iniName = "default";
                    _iniProfile = new BS_Utils.Utilities.Config("Chroma/Preferences/" + iniName);
                }
                return _iniProfile;
            }
            set {
                _iniProfile = value;
            }
        }


        public static int TimesLaunched {
            get { return timesLaunched; }
        }
        private static int timesLaunched = 0;


        private static MainSettingsModelSO mainSettingsModel;
        public static MainSettingsModelSO MainSettingsModel {
            get { return mainSettingsModel; }
        }

        public static bool oldHaptics = true;


        /// <summary>
        /// Enables debug features.  Significant performance cost.
        /// </summary>
        public static bool DebugMode {
            get { return debugMode; }
            set {
                debugMode = value;
                ChromaConfig.SetBool("Other", "debugMode", debugMode);
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
                ChromaConfig.SetFloat("Other", "sidePanel", (float)sidePanel);
            }
        }
        private static ChromaSettingsUI.SidePanelEnum sidePanel = ChromaSettingsUI.SidePanelEnum.Default;

        /// <summary>
        /// Enables checking for tailored maps.
        /// This will not disable map checking entirely, it will simply prevent a map from being detected as created for a specific gamemode.
        /// </summary>
        public static bool CustomMapCheckingEnabled {
            get { return customMapCheckingEnabled; }
            set {
                customMapCheckingEnabled = value;
                ChromaConfig.SetBool("Map", "customMapCheckingEnabled", customMapCheckingEnabled);
            }
        }
        private static bool customMapCheckingEnabled = true;

        public static bool CustomColourEventsEnabled {
            get { return customColourEventsEnabled; }
            set {
                customColourEventsEnabled = value;
                ChromaConfig.SetBool("Map", "customColourEventsEnabled", customColourEventsEnabled);
            }
        }
        private static bool customColourEventsEnabled = true;

        /// <summary>
        /// Global multiplier for audio sources used by Chroma
        /// </summary>
        public static float MasterVolume {
            get { return masterVolume; }
            set {
                masterVolume = value;
                ChromaConfig.SetFloat("Audio", "masterVolume", masterVolume);
            }
        }
        private static float masterVolume = 1f;


        /// <summary>
        /// Global multiplier for audio sources used by Chroma
        /// </summary>
        public static float SaberTrailStrength {
            get { return saberTrailStrength; }
            set {
                saberTrailStrength = value;
                ChromaConfig.SetFloat("Aesthetics", "saberTrailStrength", saberTrailStrength);
            }
        }
        private static float saberTrailStrength = 1f;


        /// <summary>
        /// Required for any features that may cause dizziness, disorientation, nausea, seizures, or other forms of discomfort.
        /// </summary>
        public static bool WaiverRead {
            get { return waiverRead; }
            set {
                if (value) {
                    waiverRead = true;
                    ChromaConfig.SetInt("Other", "safetyWaiver", 51228);
                }
            }
        }
        private static bool waiverRead = false;

        public static bool HideSubMenus {
            get { return hideSubMenus; }
            set {
                hideSubMenus = value;
                ChromaConfig.SetBool("Other", "hideSubMenus", hideSubMenus);
            }
        }
        private static bool hideSubMenus;

        #region modifiers
        public static bool LightshowModifier {
            get { return lightshowModifier; }
            set {
                lightshowModifier = value;
                ChromaConfig.SetBool("Modifiers", "lightshowModifier", lightshowModifier);
            }
        }
        private static bool lightshowModifier;
        #endregion







        #region technicolour 
        public static bool TechnicolourEnabled {
            get { return technicolourEnabled; }
            set {
                technicolourEnabled = value;
                ChromaConfig.SetBool("Technicolour", "technicolourEnabled", technicolourEnabled);
            }
        }
        private static bool technicolourEnabled = false;


        public static TechnicolourStyle TechnicolourLightsStyle {
            get {
                return technicolourLightsStyle;
            }
            set {
                technicolourLightsStyle = value;
                ChromaConfig.SetInt("Technicolour", "technicolourLightsStyle", (int)technicolourLightsStyle);
            }
        }
        private static TechnicolourStyle technicolourLightsStyle = TechnicolourStyle.OFF;


        public static TechnicolourLightsGrouping TechnicolourLightsGrouping {
            get { return technicolourLightsGrouping; }
            set {
                technicolourLightsGrouping = value;
                ChromaConfig.SetFloat("Technicolour", "technicolourLightsGrouping", (int)technicolourLightsGrouping);
            }
        }
        private static TechnicolourLightsGrouping technicolourLightsGrouping = TechnicolourLightsGrouping.STANDARD;


        public static float TechnicolourLightsFrequency {
            get { return technicolourLightsFrequency; }
            set {
                technicolourLightsFrequency = value;
                ChromaConfig.SetFloat("Technicolour", "technicolourLightsFrequency", technicolourLightsFrequency);
            }
        }
        private static float technicolourLightsFrequency = 0.1f;


        public static TechnicolourStyle TechnicolourSabersStyle {
            get {
                return technicolourSabersStyle;
            }
            set {
                technicolourSabersStyle = value;
                ChromaConfig.SetInt("Technicolour", "technicolourSabersStyle", (int)technicolourSabersStyle);
            }
        }
        private static TechnicolourStyle technicolourSabersStyle = TechnicolourStyle.OFF;

        public static TechnicolourStyle TechnicolourBlocksStyle {
            get {
                return technicolourBlocksStyle;
            }
            set {
                technicolourBlocksStyle = value;
                ChromaConfig.SetInt("Technicolour", "technicolourBlocksStyle", (int)technicolourBlocksStyle);
            }
        }
        private static TechnicolourStyle technicolourBlocksStyle = TechnicolourStyle.OFF;

        public static TechnicolourStyle TechnicolourWallsStyle {
            get {
                return technicolourWallsStyle;
            }
            set {
                technicolourWallsStyle = value;
                ChromaConfig.SetInt("Technicolour", "technicolourWallsStyle", (int)technicolourWallsStyle);
            }
        }
        private static TechnicolourStyle technicolourWallsStyle = TechnicolourStyle.OFF;

        public static TechnicolourStyle TechnicolourBombsStyle {
            get {
                return technicolourBombsStyle;
            }
            set {
                technicolourBombsStyle = value;
                ChromaConfig.SetInt("Technicolour", "technicolourBombsStyle", (int)technicolourBombsStyle);
            }
        }
        private static TechnicolourStyle technicolourBombsStyle = TechnicolourStyle.OFF;

        public static bool MatchTechnicolourSabers {
            get { return matchTechnicolourSabers; }
            set {
                matchTechnicolourSabers = value;
                ChromaConfig.SetBool("Map", "matchTechnicolourSabers", matchTechnicolourSabers);
            }
        }
        private static bool matchTechnicolourSabers = true;
        #endregion

        #region tempoary
        public static bool LegacyLighting { get { return legacyLighting; } }
        private static bool legacyLighting = false;
        #endregion

        /// <summary>
        /// Called when Chroma reloads the config files.
        /// </summary>
        public static event LoadSettingsDelegate LoadSettingsEvent;
        public delegate void LoadSettingsDelegate(BS_Utils.Utilities.Config iniProfile, LoadSettingsType type);


        internal static void Init() {
            LoadSettingsEvent += OnLoadSettingsEvent;

            ChromaPlugin.MainMenuLoadedEvent += OnMainMenuLoaded;
            ChromaPlugin.SongSceneLoadedEvent += OnSongLoaded;

            ChromaPlugin.MainMenuLoadedEvent += ChromaEvent.ClearChromaEvents;
            ChromaPlugin.SongSceneLoadedEvent += ChromaEvent.ClearChromaEvents;

            ChromaPlugin.MainMenuLoadedEvent += CleanupSongEvents;
            ChromaPlugin.SongSceneLoadedEvent += CleanupSongEvents;
        }


        private static void OnMainMenuLoaded() {
            ColourManager.RemoveNoteTypeColourOverride(NoteType.NoteA);
            ColourManager.RemoveNoteTypeColourOverride(NoteType.NoteB);

            LoadSettings(LoadSettingsType.MENU_LOADED);
        }
        private static void OnSongLoaded() {
            ColourManager.RemoveNoteTypeColourOverride(NoteType.NoteA);
            ColourManager.RemoveNoteTypeColourOverride(NoteType.NoteB);

            ColourManager.RefreshLights();
        }

        private static void CleanupSongEvents() {
            ChromaObstacleColourEvent.CustomObstacleColours.Clear();
            ChromaNoteColourEvent.CustomNoteColours.Clear();
            ChromaNoteColourEvent.SavedNoteColours.Clear();
            ChromaBombColourEvent.CustomBombColours.Clear();
            ChromaLightColourEvent.CustomLightColours.Clear();
            ChromaGradientEvent.CustomGradients.Clear();

            ChromaGradientEvent.Clear();
            VFX.TechnicolourController.Clear();

            ColourManager.LightSwitchs = null;

            Beatmap.ChromaEvents.MayhemEvent.manager = null;
        }

        internal static void LoadSettings(LoadSettingsType type) {
            //string iniName = ModPrefs.GetString("Chroma", "ConfigProfile", "default", true); //TODO get the thing
            string iniName = "settings";
            IniProfile = new BS_Utils.Utilities.Config("Chroma/Preferences/" + iniName);

            LoadSettingsEvent?.Invoke(IniProfile, type);
        }


        private static void OnLoadSettingsEvent(BS_Utils.Utilities.Config iniProfile, LoadSettingsType type) {

            try {
                
                ChromaLogger.Log("Loading settings [" + type.ToString() + "]", ChromaLogger.Level.INFO);

                //string iniName = ModPrefs.GetString("Chroma", "ConfigProfile", "default", true); //TODO get the thing
                string iniName = "settings";
                IniProfile = new BS_Utils.Utilities.Config("Chroma/Preferences/" + iniName);

                ChromaLogger.Log("--- From file " + iniName);

                BS_Utils.Gameplay.GetUserInfo.UpdateUserInfo();
                Username = BS_Utils.Gameplay.GetUserInfo.GetUserName();
                UserID = BS_Utils.Gameplay.GetUserInfo.GetUserID();

                ChromaLogger.Log(Greetings.GetGreeting(UserID, Username), ChromaLogger.Level.INFO);
                if (DebugMode) ChromaLogger.Log("=== YOUR ID : " + UserID.ToString());

                if (type == LoadSettingsType.INITIAL) {
                    timesLaunched = ChromaConfig.GetInt("Other", "timesLaunched", 0)+1;
                    ChromaConfig.SetInt("Other", "timesLaunched", timesLaunched);
                }
                

                /*
                 * MAP
                 */

                customMapCheckingEnabled = ChromaConfig.GetBool("Map", "customMapCheckingEnabled", true);
                customColourEventsEnabled = ChromaConfig.GetBool("Map", "customColourEventsEnabled", true);
                ChromaUtils.SetSongCoreCapability("Chroma Lighting Events", CustomColourEventsEnabled);

                /*
                 * AUDIO
                 */

                masterVolume = Mathf.Clamp01(ChromaConfig.GetFloat("Audio", "masterVolume", 1));

                AudioUtil.Instance.SetVolume(masterVolume);

                /*
                 * TECHNICOLOUR
                 */

                if (type == LoadSettingsType.INITIAL) {
                    technicolourEnabled = ChromaConfig.GetBool("Technicolour", "technicolourEnabled", false);

                    technicolourLightsStyle = (TechnicolourStyle)ChromaConfig.GetInt("Technicolour", "technicolourLightsStyle", 1);
                    //technicolourLightsIndividual = GetBool("Technicolour", "technicolourLightsIndividual", technicolourLightsIndividual);
                    technicolourLightsGrouping = (TechnicolourLightsGrouping)ChromaConfig.GetInt("Technicolour", "technicolourLightsGrouping", 1);
                    technicolourLightsFrequency = GetFloat("Technicolour", "technicolourLightsFrequency", technicolourLightsFrequency);
                    technicolourSabersStyle = (TechnicolourStyle)ChromaConfig.GetInt("Technicolour", "technicolourSabersStyle", 0);
                    technicolourBlocksStyle = (TechnicolourStyle)ChromaConfig.GetInt("Technicolour", "technicolourBlocksStyle", 0);
                    technicolourWallsStyle = (TechnicolourStyle)ChromaConfig.GetInt("Technicolour", "technicolourWallsStyle", 0);
                    technicolourBombsStyle = (TechnicolourStyle)ChromaConfig.GetInt("Technicolour", "technicolourBombsStyle", 0);
                    matchTechnicolourSabers = ChromaConfig.GetBool("Technicolour", "matchTechnicolourSabers", false);
                }

                string[] technicolourColdString = ChromaConfig.GetString("Technicolour", "technicolourB", "0;128;255;255-0;255;0;255-0;0;255;255-0;255;204;255").Split('-');
                string[] technicolourWarmString = ChromaConfig.GetString("Technicolour", "technicolourA", "255;0;0;255-255;0;255;255-255;153;0;255-255;0;102;255").Split('-');

                Color[] technicolourCold = new Color[technicolourColdString.Length];
                Color[] technicolourWarm = new Color[technicolourWarmString.Length];

                for (int i = 0; i < Mathf.Max(technicolourCold.Length, technicolourWarm.Length); i++) {
                    if (i < technicolourCold.Length) {
                        technicolourCold[i] = ColourManager.ColourFromString(technicolourColdString[i]);
                    }
                    if (i < technicolourWarm.Length) {
                        technicolourWarm[i] = ColourManager.ColourFromString(technicolourWarmString[i]);
                    }
                }

                ColourManager.TechnicolourWarmPalette = technicolourWarm;
                ColourManager.TechnicolourColdPalette = technicolourCold;

                /*
                 * NOTES
                 */

                //ColourManager.A = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Notes", "colourA", "DEFAULT"), ColourManager.DefaultA);
                //ColourManager.B = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Notes", "colourB", "DEFAULT"), ColourManager.DefaultB);
                ColourManager.AltA = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Notes", "colourAltA", "Notes Magenta"), ColourManager.DefaultAltA);
                ColourManager.AltB = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Notes", "colourAltB", "Notes Green"), ColourManager.DefaultAltB);
                ColourManager.NonColoured = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Notes", "colourNonColoured", "Notes White"), ColourManager.DefaultNonColoured);
                ColourManager.DoubleHit = ColourManager.DoubleHit = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Notes", "colourDuochrome", "Notes Purple"));
                ColourManager.Super = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Notes", "colourSuper", "Notes Gold"), ColourManager.DefaultSuper);

                /*
                 * LIGHTS
                 */

                //ColourManager.LightAmbient = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Lights", "lightAmbient", "DEFAULT"), ColourManager.DefaultLightAmbient);
                //ColourManager.LightA = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Lights", "lightColourA", "DEFAULT"), ColourManager.DefaultLightA);
                //ColourManager.LightB = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Lights", "lightColourB", "DEFAULT"), ColourManager.DefaultLightB);
                ColourManager.LightAltA = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Lights", "lightColourAltA", "Light Magenta"), ColourManager.DefaultLightAltA);
                ColourManager.LightAltB = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Lights", "lightColourAltB", "Light Green"), ColourManager.DefaultLightAltB);
                ColourManager.LightWhite = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Lights", "lightColourWhite", "Light White"), ColourManager.DefaultLightWhite);
                ColourManager.LightGrey = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Lights", "lightColourGrey", "Light Grey"), ColourManager.DefaultLightGrey);

                /*
                 * AESTHETICS
                 */

                //ColourManager.BarrierColour = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Aesthetics", "barrierColour", "DEFAULT"), ColourManager.DefaultBarrierColour);
                ColourManager.LaserPointerColour = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Aesthetics", "laserPointerColour", "DEFAULT"), ColourManager.DefaultB);
                ColourManager.SignA = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Aesthetics", "signColourA", "DEFAULT"), ColourManager.DefaultA);
                ColourManager.SignB = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Aesthetics", "signColourB", "DEFAULT"), ColourManager.DefaultB);
                ColourManager.Platform = ChromaSettingsUI.GetColor(ChromaConfig.GetString("Aesthetics", "platformAccoutrements", "DEFAULT"), ColourManager.DefaultB);

                ChromaConfig.saberTrailStrength = ChromaConfig.GetFloat("Aesthetics", "saberTrailStrength", 1f);

                /*
                 * MODIFIERS
                 */
                lightshowModifier = ChromaConfig.GetBool("Modifiers", "lightshowModifier", false);

                /*
                 * OTHER
                 */

                sidePanel = (ChromaSettingsUI.SidePanelEnum)ChromaConfig.GetFloat("Other", "sidePanel", 1);

                legacyLighting = ChromaConfig.GetBool("Other", "legacyLighting", false);

                debugMode = ChromaConfig.GetBool("Other", "debugMode", false);

                hideSubMenus = ChromaConfig.GetBool("Other", "hideSubMenus", false);

                waiverRead = ChromaConfig.GetInt("Other", "safetyWaiver", 0) == 51228;

                ColourManager.RefreshLights();

                if (type == LoadSettingsType.MANUAL) AudioUtil.Instance.PlayOneShotSound("ConfigReload.wav");

            } catch (Exception e) {
                ChromaLogger.Log("Error loading Chroma configs!  Waduhek", ChromaLogger.Level.ERROR);
                ChromaLogger.Log(e);
            }

        }

        public static void LoadSettingsModel() {

            mainSettingsModel = UnityEngine.Resources.FindObjectsOfTypeAll<MainSettingsModelSO>().FirstOrDefault();
            if (mainSettingsModel) {
                ChromaLogger.Log("Found settings model", ChromaLogger.Level.DEBUG);
                oldHaptics = mainSettingsModel.controllersRumbleEnabled;
            }

        }


        #region configshortcuts

        public static void SetBool(string category, string name, bool value) {
            IniProfile.SetBool(category, name, value);
        }

        public static void SetFloat(string category, string name, float value) {
            IniProfile.SetFloat(category, name, value);
        }

        public static void SetInt(string category, string name, int value) {
            IniProfile.SetInt(category, name, value);
        }

        public static void SetString(string category, string name, string value) {
            IniProfile.SetString(category, name, value);
        }

        public static bool GetBool(string category, string name, bool def = false) {
            return IniProfile.GetBool(category, name, def, true);
        }

        public static float GetFloat(string category, string name, float def) {
            return IniProfile.GetFloat(category, name, def, true);
        }

        public static int GetInt(string category, string name, int def) {
            return IniProfile.GetInt(category, name, def, true);
        }

        public static string GetString(string category, string name, string def) {
            return IniProfile.GetString(category, name, def, true);
        }

        #endregion

    }

}
