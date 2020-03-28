using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.MenuButtons;
using BeatSaberMarkupLanguage.Settings;
using Chroma.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Chroma.ColourManager;

namespace Chroma.Settings
{
    internal class ChromaSettingsUI : PersistentSingleton<ChromaSettingsUI>
    {
        [UIValue("sidepanelchoices")]
        private List<object> _sidePanelChoices = (new object[] { ChromaConfig.SidePanelEnum.DEFAULT, ChromaConfig.SidePanelEnum.CHROMA, ChromaConfig.SidePanelEnum.CHROMAWAIVER }).ToList();

        [UIValue("mastervolumechoices")]
        private List<object> _masterVolumeChoices = new List<object>() { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f };

        [UIValue("loggerlevelchoices")]
        private List<object> _loggerLevelChoices = new List<object>() { 0, 1, 2, 3 };

        [UIValue("techlightschoices")]
        private List<object> _techlightsChoices = (new object[] { TechnicolourStyle.OFF, TechnicolourStyle.WARM_COLD, TechnicolourStyle.ANY_PALETTE, TechnicolourStyle.PURE_RANDOM, TechnicolourStyle.GRADIENT }).ToList();

        [UIValue("techbarrierschoices")]
        private List<object> _techbarrierschoices = (new object[] { TechnicolourStyle.OFF, TechnicolourStyle.ANY_PALETTE, TechnicolourStyle.PURE_RANDOM, TechnicolourStyle.GRADIENT }).ToList();

        [UIValue("lightsgroupchoices")]
        private List<object> _lightsgroupChoices = new List<object>() { TechnicolourLightsGrouping.STANDARD, TechnicolourLightsGrouping.ISOLATED_GROUP, TechnicolourLightsGrouping.ISOLATED };

        [UIValue("lightsfreqchoices")]
        private List<object> _lightsfreqChoices = new List<object>() { 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1f };

        [UIValue("colours")]
        private List<object> _colours = colourPresets.Cast<object>().ToList();

        [UIAction("colourformat")]
        private string colourFormat(NamedColor col)
        {
            return col.name;
        }

        [UIAction("sidepanelform")]
        private string sidePanelFormat(ChromaConfig.SidePanelEnum f)
        {
            switch (f)
            {
                case ChromaConfig.SidePanelEnum.CHROMA:
                    return "Chroma Notes";

                case ChromaConfig.SidePanelEnum.CHROMAWAIVER:
                    return "Safety Waiver";

                case ChromaConfig.SidePanelEnum.DEFAULT:
                default:
                    return "Release Notes";
            }
        }

        [UIAction("techlightform")]
        private string techlightFormat(TechnicolourStyle t)
        {
            switch (t)
            {
                case TechnicolourStyle.GRADIENT:
                    return "GRADIENT";

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
        private string techgroupingFormat(TechnicolourLightsGrouping t)
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
        private string percentDisplay(float percent)
        {
            return $"{percent * 100f}%";
        }

        [UIAction("percentfreq")]
        private string percentfreqDisplay(float percent)
        {
            return percentDisplay(percent) + (percent == 0.1f ? " (Def)" : "");
        }

        #region Settings

        [UIValue("sidepanel")]
        public ChromaConfig.SidePanelEnum SidePanel
        {
            get => ChromaConfig.SidePanel;
            set
            {
                ChromaConfig.SidePanel = value;
                SidePanelUtil.SetPanel(Enum.GetName(typeof(ChromaConfig.SidePanelEnum), ChromaConfig.SidePanel));
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

        [UIValue("secrets")]
        /// <summary>
        /// Required for any features that may cause dizziness, disorientation, nausea, seizures, or other forms of discomfort.
        /// </summary>
        public string Secrets
        {
            get => "";
            set
            {
                switch (value.ToUpper())
                {
                    case "SAFETYHAZARD":
                        ChromaConfig.WaiverRead = true;
                        break;

                    case "LIGHTSHOW":
                        BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup.instance.AddTab("Lightshow Modifiers", "Chroma.Settings.lightshow.bsml", instance);
                        ChromaConfig.LightshowMenu = true;
                        break;
                }
            }
        }

        #endregion Settings

        #region Aesthetics

        [UIValue("signcolourb")]
        public NamedColor NeonSignTop
        {
            get => ChromaConfig.NeonSignTop;
            set
            {
                ChromaConfig.NeonSignTop = value;
                RecolourNeonSign(SignA, SignB);
            }
        }

        [UIValue("signcoloura")]
        public NamedColor NeonSignBottom
        {
            get => ChromaConfig.NeonSignBottom;
            set
            {
                ChromaConfig.NeonSignBottom = value;
                RecolourNeonSign(SignA, SignB);
            }
        }

        [UIValue("laserpointercolour")]
        public NamedColor LaserPointer
        {
            get => ChromaConfig.LaserPointer;
            set
            {
                ChromaConfig.LaserPointer = value;
                RecolourMenuStuff(Platform, LaserPointerColour);
            }
        }

        [UIValue("platformaccoutrements")]
        public NamedColor PlatformAccoutrements
        {
            get => ChromaConfig.PlatformAccoutrements;
            set
            {
                ChromaConfig.PlatformAccoutrements = value;
                RecolourMenuStuff(Platform, LaserPointerColour);
            }
        }

        #endregion Aesthetics

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
            get => !ChromaConfig.CustomColourEventsEnabled;
            set
            {
                ChromaConfig.CustomColourEventsEnabled = !value;
                ChromaUtils.SetSongCoreCapability(Plugin.REQUIREMENT_NAME, !value);
            }
        }

        [UIValue("notecolours")]
        public bool NoteColourEventsEnabled
        {
            get => !ChromaConfig.NoteColourEventsEnabled;
            set
            {
                ChromaConfig.NoteColourEventsEnabled = !value;
            }
        }

        #endregion Events

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
        public TechnicolourStyle TechnicolourWallsStyle
        {
            get => ChromaConfig.TechnicolourWallsStyle;
            set
            {
                ChromaConfig.TechnicolourWallsStyle = value;
            }
        }

        [UIValue("techbombs")]
        public TechnicolourStyle TechnicolourBombsStyle
        {
            get => ChromaConfig.TechnicolourBombsStyle;
            set
            {
                ChromaConfig.TechnicolourBombsStyle = value;
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

        #endregion Technicolour

        #region Lightshow

        [UIValue("playersplace")]
        public bool PlayersPlace
        {
            get => ChromaConfig.PlayersPlace;
            set
            {
                ChromaConfig.PlayersPlace = value;
            }
        }

        [UIValue("spectrograms")]
        public bool Spectrograms
        {
            get => ChromaConfig.Spectrograms;
            set
            {
                ChromaConfig.Spectrograms = value;
            }
        }

        [UIValue("backcolumns")]
        public bool BackColumns
        {
            get => ChromaConfig.BackColumns;
            set
            {
                ChromaConfig.BackColumns = value;
            }
        }

        [UIValue("buildings")]
        public bool Buildings
        {
            get => ChromaConfig.Buildings;
            set
            {
                ChromaConfig.Buildings = value;
            }
        }

        #endregion Lightshow

        internal static NamedColor StringToColour(string str)
        {
            return colourPresets.FirstOrDefault(n => n.name == str);
        }

        internal static string floatToPanel(float f)
        {
            switch (f)
            {
                case 1:
                    return "chroma";

                case 0:
                default:
                    return "default";
            }
        }

        internal static List<NamedColor> colourPresets = new List<NamedColor> {
            new NamedColor( "DEFAULT", null),
            new NamedColor( "Notes Red", new Color(1, 0, 0, 1) ),
            new NamedColor( "Notes Blue", new Color(0, 0.502f, 1, 1) ),
            new NamedColor( "Notes Magenta", new Color(1, 0, 1, 1) ),
            new NamedColor( "Notes Green", new Color(0, 1, 0, 1) ),
            new NamedColor( "Notes Purple", new Color(1.05f, 0, 2.188f, 1) ),
            new NamedColor( "Notes White", new Color(1, 1, 1, 1) ),
            new NamedColor( "Notes Gold", new Color(1, 1, 0, 1) ),

            new NamedColor( "Light Ambient", new Color(0, 0.706f, 1f, 1) ),
            new NamedColor( "Light Red", new Color(1, 0.016f, 0.016f, 1) ),
            new NamedColor( "Light Blue", new Color(0, 0.753f, 1, 1) ),
            new NamedColor( "Light Magenta", new Color(1, 0.032f, 1, 1) ),
            new NamedColor( "Light Green", new Color(0.016f, 1, 0.016f, 1) ),
            new NamedColor( "Light White", new Color(1, 1, 1, 1) ),
            new NamedColor( "Light Grey", new Color(0.6f, 0.6f, 0.6f, 1) ),

            new NamedColor( "Barrier Red", Color.red ),

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
        };
    }
}