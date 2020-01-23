using Chroma.Utils;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.MenuButtons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Chroma.ColourManager;

namespace Chroma.Settings {

    public class ChromaSettingsUI : PersistentSingleton<ChromaSettingsUI> {

        [UIValue("sidepanelchoices")]
        private List<object> _sidePanelChoices = (new object[] { SidePanelEnum.Default, SidePanelEnum.Chroma, SidePanelEnum.ChromaWaiver }).ToList();
        public enum SidePanelEnum
        {
            Default = 0,
            Chroma = 1,
            ChromaWaiver = 2
        }

        [UIValue("mastervolumechoices")]
        private List<object> _masterVolumeChoices = new List<object>() { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f };

        [UIValue("loggerlevelchoices")]
        private List<object> _loggerLevelChoices = new List<object>() { 0, 1, 2, 3 };

        [UIValue("barrierccscalechoices")]
        private List<object> _barrierccChoices = new List<object>() { 0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f };

        [UIValue("techlightschoices")]
        private List<object> _techlightsChoices = (new object[] { TechnicolourStyle.OFF, TechnicolourStyle.WARM_COLD, TechnicolourStyle.ANY_PALETTE, TechnicolourStyle.PURE_RANDOM }).ToList();

        [UIValue("lightsgroupchoices")]
        private List<object> _lightsgroupChoices = ChromaConfig.WaiverRead ? new List<object>() { TechnicolourLightsGrouping.STANDARD, TechnicolourLightsGrouping.ISOLATED_GROUP, TechnicolourLightsGrouping.ISOLATED }
            : new List<object>() { TechnicolourLightsGrouping.STANDARD, TechnicolourLightsGrouping.ISOLATED_GROUP };

        [UIValue("lightsfreqchoices")]
        private List<object> _lightsfreqChoices = new List<object>() { 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1f };

        [UIValue("colours")]
        private List<object> _colours = colourToObject();

        [UIAction("colourformat")]
        public string colourFormat(NamedColor col)
        {
            return col.name;
        }

        [UIAction("sidepanelform")]
        public string sidePanelFormat(SidePanelEnum f)
        {
            switch (f)
            {
                case SidePanelEnum.ChromaWaiver:
                    return "Safety Waiver";
                case SidePanelEnum.Chroma:
                    return "Chroma Notes";
                case SidePanelEnum.Default:
                default:
                    return "Release Notes";
            }
        }

        [UIAction("techlightform")]
        public string techlightFormat(TechnicolourStyle t)
        {
            switch (t)
            {
                case TechnicolourStyle.PURE_RANDOM:
                    return "TRUE RANDOM";
                case TechnicolourStyle.ANY_PALETTE:
                    return "EITHER";
                case TechnicolourStyle.WARM_COLD:
                    return "WARM/COLD";
                case TechnicolourStyle.OFF:
                default:
                    return "OFF";
            }
        }

        [UIAction("techgroupform")]
        public string techgroupingFormat(TechnicolourLightsGrouping t)
        {
            switch (t)
            {
                case TechnicolourLightsGrouping.ISOLATED:
                    return "Isolated (Mayhem)";
                case TechnicolourLightsGrouping.ISOLATED_GROUP:
                    return "Isolated Event";
                case TechnicolourLightsGrouping.STANDARD:
                default:
                    return "Standard";
            }
        }

        [UIAction("percent")]
        public string percentDisplay(float percent)
        {
            return $"{percent * 100f}%";
        }

        [UIAction("percentfreq")]
        public string percentfreqDisplay(float percent)
        {
            return percentDisplay(percent) + (percent==0.1f?" (Def)":"");
        }

        #region Settings
        [UIValue("sidepanel")]
        public SidePanelEnum SidePanel
        {
            get => ChromaConfig.SidePanel;
            set
            {
                ChromaConfig.SidePanel = value;
                SidePanelUtil.SetPanel(floatToPanel((float)value));
            }
        }

        [UIValue("custommapchecking")]
        /// <summary>
        /// Enables checking for tailored maps.
        /// This will not disable map checking entirely, it will simply prevent a map from being detected as created for a specific gamemode.
        /// </summary>
        public bool CustomMapCheckingEnabled
        {
            get => ChromaConfig.CustomMapCheckingEnabled;
            set
            {
                ChromaConfig.CustomMapCheckingEnabled = value;
            }
        }

        [UIValue("mastervolume")]
        /// <summary>
        /// Global multiplier for audio sources used by Chroma
        /// </summary>
        public float MasterVolume
        {
            get => ChromaConfig.MasterVolume;
            set
            {
                ChromaConfig.MasterVolume = value;
            }
        }

        [UIValue("debugmode")]
        /// <summary>
        /// Global multiplier for audio sources used by Chroma
        /// </summary>
        public bool DebugMode
        {
            get => ChromaConfig.DebugMode;
            set
            {
                ChromaConfig.DebugMode = value;
            }
        }

        [UIValue("loggerlevel")]
        public int LogLevel
        {
            get => ChromaConfig.GetInt("Logger", "loggerLevel", 2);
            set
            {
                ChromaConfig.SetInt("Logger", "loggerLevel", value);
                ChromaLogger.LogLevel = (ChromaLogger.Level)value;
            }
        }

        [UIValue("secrets")]
        /// <summary>
        /// Required for any features that may cause dizziness, disorientation, nausea, seizures, or other forms of discomfort.
        /// </summary>
        public string Secrets
        {
            get => secret;
            set
            {
                if (value.ToUpper() == "SAFETYHAZARD")
                {
                    ChromaConfig.WaiverRead = true;
                    AudioUtil.Instance.PlayOneShotSound("NightmareMode.wav");
                }
                else if (value.ToUpper() == "CREDITS")
                {
                    AudioUtil.Instance.PlayOneShotSound("ConfigReload.wav");
                }
                secret = value;
            }
        }
        private static string secret = "";
        #endregion

        #region Lights/Notes
        [UIValue("coloura")]
        public NamedColor LeftNotes
        {
            get => stringToColour(ChromaConfig.GetString("Notes", "colourA", "DEFAULT"));
            set
            {
                ColourManager.A = value.color;
                ChromaConfig.SetString("Notes", "colourA", value.name);
            }
        }

        [UIValue("colourb")]
        public NamedColor RightNotes
        {
            get => stringToColour(ChromaConfig.GetString("Notes", "colourB", "DEFAULT"));
            set
            {
                ColourManager.B = value.color;
                ChromaConfig.SetString("Notes", "colourB", value.name);
            }
        }

        [UIValue("lightambient")]
        public NamedColor AmbientLights
        {
            get => stringToColour(ChromaConfig.GetString("Lights", "lightAmbient", "DEFAULT"));
            set
            {
                ColourManager.LightAmbient = value.color;
                ColourManager.RecolourAmbientLights(ColourManager.LightAmbient);
                ChromaConfig.SetString("Lights", "lightAmbient", value.name);
            }
        }

        [UIValue("lightcoloura")]
        public NamedColor WarmLights
        {
            get => stringToColour(ChromaConfig.GetString("Lights", "lightColourA", "DEFAULT"));
            set
            {
                ColourManager.LightA = value.color;
                ColourManager.RecolourAmbientLights(ColourManager.LightAmbient);
                ChromaConfig.SetString("Lights", "lightColourA", value.name);
            }
        }

        [UIValue("lightcolourb")]
        public NamedColor ColdLights
        {
            get => stringToColour(ChromaConfig.GetString("Lights", "lightColourB", "DEFAULT"));
            set
            {
                ColourManager.LightB = value.color;
                ColourManager.RecolourAmbientLights(ColourManager.LightAmbient);
                ChromaConfig.SetString("Lights", "lightColourB", value.name);
            }
        }
        #endregion

        #region Aesthetics
        [UIValue("barriercolour")]
        public NamedColor Barriers
        {
            get => stringToColour(ChromaConfig.GetString("Aesthetics", "barrierColour", "Barrier Red"));
            set
            {
                ChromaConfig.SetString("Aesthetics", "barrierColour", value.name);
            }
        }

        [UIValue("barrierccscale")]
        public float BarrierColCorrection
        {
            get => ChromaConfig.GetFloat("Aesthetics", "barrierColourCorrectionScale", 1f);
            set
            {
                ColourManager.barrierColourCorrectionScale = value;
                ChromaConfig.SetFloat("Aesthetics", "barrierColourCorrectionScale", value);
            }
        }

        [UIValue("signcolourb")]
        public NamedColor NeonSignTop
        {
            get => stringToColour(ChromaConfig.GetString("Aesthetics", "signColourB", "DEFAULT"));
            set
            {
                ColourManager.SignB = value.color;
                ColourManager.RecolourNeonSign(ColourManager.SignA, ColourManager.SignB);
                ChromaConfig.SetString("Aesthetics", "signcolourB", value.name);
            }
        }

        [UIValue("signcoloura")]
        public NamedColor NeonSignBottom
        {
            get => stringToColour(ChromaConfig.GetString("Aesthetics", "signColourA", "DEFAULT"));
            set
            {
                ColourManager.SignB = value.color;
                ColourManager.RecolourNeonSign(ColourManager.SignA, ColourManager.SignB);
                ChromaConfig.SetString("Aesthetics", "signColourA", value.name);
            }
        }

        [UIValue("laserpointercolour")]
        public NamedColor LaserPointer
        {
            get => stringToColour(ChromaConfig.GetString("Aesthetics", "laserPointerColour", "DEFAULT"));
            set
            {
                ColourManager.LaserPointerColour = value.color;
                ColourManager.RecolourMenuStuff(ColourManager.A, ColourManager.B, ColourManager.LightA, ColourManager.LightB, ColourManager.Platform, ColourManager.LaserPointerColour);
                ChromaConfig.SetString("Aesthetics", "laserPointerColour", value.name);
            }
        }

        [UIValue("platformaccoutrements")]
        public NamedColor PlatformAccoutrements
        {
            get => stringToColour(ChromaConfig.GetString("Aesthetics", "platformAccoutrements", "DEFAULT"));
            set
            {
                ColourManager.Platform = value.color;
                ColourManager.RecolourMenuStuff(ColourManager.A, ColourManager.B, ColourManager.LightA, ColourManager.LightB, ColourManager.Platform, ColourManager.LaserPointerColour);
                ChromaConfig.SetString("Aesthetics", "platformAccoutrements", value.name);
            }
        }
        #endregion

        #region Events
        [UIValue("lightshowonly")]
        public bool LightshowModifier
        {
            get => ChromaConfig.LightshowModifier;
            set
            {
                ChromaConfig.LightshowModifier = value;
            }
        }

        [UIValue("rgbevents")]
        public bool CustomColourEventsEnabled
        {
            get => ChromaConfig.CustomColourEventsEnabled;
            set
            {
                ChromaConfig.CustomColourEventsEnabled = value;
                ChromaPlugin.SetRGBCapability(value);
            }
        }

        [UIValue("specialevents")]
        public bool CustomSpecialEventsEnabled
        {
            get => ChromaConfig.CustomSpecialEventsEnabled;
            set
            {
                ChromaConfig.CustomSpecialEventsEnabled = value;
                ChromaPlugin.SetSpecialEventCapability(value);
            }
        }
        #endregion

        #region Technicolour
        [UIValue("technicolour")]
        public bool TechnicolourEnabled
        {
            get => ChromaConfig.TechnicolourEnabled;
            set
            {
                ChromaConfig.TechnicolourEnabled = value;
            }
        }

        [UIValue("techlights")]
        public TechnicolourStyle TechnicolourLightsStyle
        {
            get => ChromaConfig.TechnicolourLightsStyle;
            set
            {
                ChromaConfig.TechnicolourLightsStyle = value;
            }
        }

        [UIValue("lightsgroup")]
        public TechnicolourLightsGrouping TechnicolourLightsGroup
        {
            get => ChromaConfig.TechnicolourLightsGrouping;
            set
            {
                ChromaConfig.TechnicolourLightsGrouping = value;
            }
        }

        [UIValue("lightsfreq")]
        public float TechnicolourLightsFrequency
        {
            get => ChromaConfig.TechnicolourLightsFrequency;
            set
            {
                ChromaConfig.TechnicolourLightsFrequency = value;
            }
        }

        [UIValue("techbarriers")]
        public bool TechnicolourWallsStyle
        {
            get => ChromaConfig.TechnicolourWallsStyle == TechnicolourStyle.ANY_PALETTE ? true : false;
            set
            {
                ChromaConfig.TechnicolourWallsStyle = value ? TechnicolourStyle.ANY_PALETTE : TechnicolourStyle.OFF;
            }
        }

        [UIValue("technotes")]
        public TechnicolourStyle TechnicolourBlocksStyle
        {
            get => ChromaConfig.TechnicolourBlocksStyle;
            set
            {
                ChromaConfig.TechnicolourBlocksStyle = value;
            }
        }

        [UIValue("techsabers")]
        public TechnicolourStyle TechnicolourSabersStyle
        {
            get => ChromaConfig.TechnicolourSabersStyle;
            set
            {
                ChromaConfig.TechnicolourSabersStyle = value;
            }
        }

        [UIValue("matchsabers")]
        public bool MatchTechnicolourSabers
        {
            get => !ChromaConfig.MatchTechnicolourSabers;
            set
            {
                ChromaConfig.MatchTechnicolourSabers = !value;
            }
        }
        #endregion

        private static NamedColor stringToColour(string str)
        {
            if (colourPresets==null) InitializePresetList();
            foreach (NamedColor t in colourPresets)
            {
                if (t.name == str) return t;
            }
            return colourPresets[0];
        }

        private static List<object> colourToObject()
        {
            if (colourPresets == null) InitializePresetList();
            List<object> t = new List<object>();
            foreach (NamedColor i in colourPresets)
            {
                t.Add(i);
            }
            return t;
        }

        public static string floatToPanel(float f)
        {
            switch (f)
            {
                case 2:
                    return "chromaWaiver";
                case 1:
                    return "chroma";
                case 0:
                default:
                    return "default";
            }
        }

        public static void OnReloadClick() {
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            ChromaConfig.LoadSettings(ChromaConfig.LoadSettingsType.MANUAL);
        }

        public static void InitializeMenu() {
            InitializePresetList();

            ChromaLogger.Log("Registering buttons");
            
            MenuButtons.instance.RegisterButton(new MenuButton("Reload Chroma", "", OnReloadClick, true));
            //MenuButtons.instance.RegisterButton(new MenuButton("Show Release Notes", "Shows the Release Notes and other info from the Beat Saber developers", delegate { SidePanelUtil.ResetPanel(); }, true));
            //MenuButtons.instance.RegisterButton(new MenuButton("Chroma Notes", "Shows the Release Notes and other info for Chroma", delegate { SidePanelUtil.SetPanel("chroma"); }, true));
            //MenuButtons.instance.RegisterButton(new MenuButton("Safety Waiver", "Shows the Chroma Safety Waiver", delegate { SidePanelUtil.SetPanel("chromaWaiver"); }, true));
        }

        //private static List<Tuple<string, Color>> colourPresets = null;

        private static List<NamedColor> colourPresets = null;// = new List<NamedColour>();

        public static Color GetColor(string name) {
            return GetColor(name, Color.clear);
        }

        public static Color GetColor(string name, Color defaultColor) {
            if (colourPresets == null) InitializePresetList();
            foreach (NamedColor t in colourPresets) {
                if (t.name == name) return t.color;
            }
            return defaultColor;
        }
        /*
            MultiSelectOption techniLights = GameplaySettingsUI.CreateListOption(GameplaySettingsPanels.PlayerSettingsRight, "Tech. Lights", "CTT", "Technicolour style of the lights.");
            for (int i = 0; i < technicolourOptions.Count; i++) techniLights.AddOption(i, technicolourOptions[i].Item2);
            techniLights.GetValue += delegate {
                return (int)ChromaConfig.TechnicolourLightsStyle;
            };
            techniLights.OnChange += delegate (float value) {
                ColourManager.TechnicolourStyle style = ColourManager.GetTechnicolourStyleFromFloat(value);
                ChromaConfig.TechnicolourLightsStyle = style;
            };
            techniLightsGrouping.OnChange += delegate (float value) {
                ChromaConfig.TechnicolourLightsGrouping = ColourManager.GetTechnicolourLightsGroupingFromFloat(value);
            };

            //Walls don't need to have other options since they only work nicely with Either
            ToggleOption techniWalls = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.PlayerSettingsRight, "Tech. Barriers", "CTT", "If enabled, Barriers will rainbowify!");
            techniWalls.GetValue = ChromaConfig.TechnicolourWallsStyle == ColourManager.TechnicolourStyle.ANY_PALETTE;
            techniWalls.OnToggle += delegate (bool value) {
                ChromaConfig.TechnicolourWallsStyle = value ? ColourManager.TechnicolourStyle.ANY_PALETTE : ColourManager.TechnicolourStyle.OFF;
            };


        }*/


        private static void InitializePresetList() {

            colourPresets = new List<NamedColor>() { new NamedColor( "DEFAULT", Color.clear ) };// new List<Tuple<string, Color>>();

            ColourManager.SaveExampleColours();

            //TODO add custom colours
            List<NamedColor> userColours = ColourManager.LoadColoursFromFile();
            if (userColours != null) {
                foreach (NamedColor t in userColours) {
                    colourPresets.Add(t);
                }
            }

            // CC GitHub to steal colours from
            // https://github.com/Kylemc1413/BeatSaber-CustomColors/blob/master/ColorsUI.cs
            foreach (NamedColor t in new List<NamedColor> {
                new NamedColor( "Notes Red", ColourManager.DefaultA ),
                new NamedColor( "Notes Blue", ColourManager.DefaultB ),
                new NamedColor( "Notes Magenta", ColourManager.DefaultAltA ),
                new NamedColor( "Notes Green", ColourManager.DefaultAltB ),
                new NamedColor( "Notes Purple", ColourManager.DefaultDoubleHit ),
                new NamedColor( "Notes White", ColourManager.DefaultNonColoured ),
                new NamedColor( "Notes Gold", ColourManager.DefaultSuper ),

                new NamedColor( "Light Ambient", ColourManager.DefaultLightAmbient ),
                new NamedColor( "Light Red", ColourManager.DefaultLightA ),
                new NamedColor( "Light Blue", ColourManager.DefaultLightB ),
                new NamedColor( "Light Magenta", ColourManager.DefaultLightAltA ),
                new NamedColor( "Light Green", ColourManager.DefaultLightAltB ),
                new NamedColor( "Light White", ColourManager.DefaultLightWhite ),
                new NamedColor( "Light Grey", ColourManager.DefaultLightGrey ),

                new NamedColor( "Barrier Red", ColourManager.DefaultBarrierColour ),

                new NamedColor( "CC Elec. Blue", new Color(0, .98f, 2.157f) ),
                new NamedColor( "CC Dark Blue", new Color(0f, 0.28000000000000003f, 0.55000000000000004f) ),
                new NamedColor( "CC Purple", new Color(1.05f, 0, 2.188f) ),
                new NamedColor( "CC Orange", new Color(2.157f ,.588f, 0) ),
                new NamedColor( "CC Yellow", new Color(2.157f, 1.76f, 0) ),
                new NamedColor( "CC Dark", new Color(0.3f, 0.3f, 0.3f) ),
                new NamedColor( "CC Black", new Color(0f, 0f, 0f) ),

                new NamedColor( "K/DA Orange", new Color(1.000f, 0.396f, 0.243f) ),
                new NamedColor( "K/DA Purple", new Color(0.761f, 0.125f, 0.867f) ),
                new NamedColor( "Klouder Blue", new Color(0.349f, 0.69f, 0.957f) ),
                new NamedColor( "Miku", new Color(0.0352941176f, 0.929411765f, 0.764705882f) ),
            }) {
                colourPresets.Add(t);
            }

        }


    }

}
