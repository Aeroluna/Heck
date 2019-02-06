using Chroma.Utils;
using CustomUI.GameplaySettings;
using CustomUI.MenuButton;
using CustomUI.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chroma.Settings {

    public class ChromaSettingsUI {

        public delegate void SettingsMenuCreatedDelegate(SubMenu ctSettings);
        public static event SettingsMenuCreatedDelegate SettingsMenuCreatedEvent;

        //public delegate void SettingsSubMenuCreatedDelegate(SubMenu subMenu);
        //public static event SettingsSubMenuCreatedDelegate SettingsSubMenuCreatedEvent;

        public delegate void SettingsNoteMenuAddedDelegate(SubMenu subMenu, float[] presets, List<NamedColor> colourPresets);
        public static event SettingsNoteMenuAddedDelegate SettingsNoteMenuAddedEvent;

        public delegate void SettingsLightsMenuAddedDelegate(SubMenu subMenu, float[] presets, List<NamedColor> colourPresets);
        public static event SettingsLightsMenuAddedDelegate SettingsLightsMenuAddedEvent;

        public delegate void SettingsOthersMenuAddedDelegate(SubMenu subMenu, float[] presets, List<NamedColor> colourPresets);
        public static event SettingsOthersMenuAddedDelegate SettingsOthersMenuAddedEvent;

        public delegate void ExtensionSubMenusDelegate(float[] presets, List<NamedColor> colourPresets);
        public static event ExtensionSubMenusDelegate ExtensionSubMenusEvent;

        public static void OnReloadClick() {
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            ChromaConfig.LoadSettings(ChromaConfig.LoadSettingsType.MANUAL);
        }

        public static void InitializeMenu() {

            InitializePresetList();

            MenuButtonUI.AddButton("Reload Chroma", OnReloadClick);
            MenuButtonUI.AddButton("Show Release Notes", "Shows the Release Notes and other info from the Beat Saber developers", delegate { SidePanelUtil.ResetPanel(); });
            MenuButtonUI.AddButton("Chroma Notes", "Shows the Release Notes and other info for Chroma", delegate { SidePanelUtil.SetPanel("chroma"); });
            MenuButtonUI.AddButton("Safety Waiver", "Shows the Chroma Safety Waiver", delegate { SidePanelUtil.SetPanel("chromaWaiver"); });

            /*
             * SETTINGS
             */
            SubMenu ctSettings = SettingsUI.CreateSubMenu("Chroma Settings");
            BoolViewController hideSubMenusController = ctSettings.AddBool("Hide CT Menus", "If true, hides all other Chroma menus.  This has a lot of options, I know.");
            hideSubMenusController.GetValue += delegate { return ChromaConfig.HideSubMenus; };
            hideSubMenusController.SetValue += delegate (bool value) { ChromaConfig.HideSubMenus = value; };

            BoolViewController ctSettingsMapCheck = ctSettings.AddBool("Enabled Map Checking", "If false, Chroma and its extensions will not check for special maps.  Recommended to leave on.");
            ctSettingsMapCheck.GetValue += delegate { return ChromaConfig.CustomMapCheckingEnabled; };
            ctSettingsMapCheck.SetValue += delegate (bool value) { ChromaConfig.CustomMapCheckingEnabled = value; };

            ListViewController ctSettingsMasterVolume = ctSettings.AddList("Chroma Sounds Volume", new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f }, "Master volume control for sounds made by Chroma");
            ctSettingsMasterVolume.GetValue += delegate { return ChromaConfig.MasterVolume; };
            ctSettingsMasterVolume.SetValue += delegate (float value) { ChromaConfig.MasterVolume = value; };
            ctSettingsMasterVolume.FormatValue += delegate (float value) { return value * 100 + "%"; };

            BoolViewController ctSettingsDebugMode = ctSettings.AddBool("Debug Mode", "Very performance heavy - only do this if you are bug chasing.");
            ctSettingsDebugMode.GetValue += delegate { return ChromaConfig.DebugMode; };
            ctSettingsDebugMode.SetValue += delegate (bool value) { ChromaConfig.DebugMode = value; };

            ListViewController ctLogSettings = ctSettings.AddList("Logging Level", new float[] { 0, 1, 2, 3 }, "The further to the left this is, the more will be logged.");
            ctLogSettings.applyImmediately = true;
            ctLogSettings.GetValue += delegate { return (int)ChromaLogger.LogLevel; };
            ctLogSettings.SetValue += delegate (float value) { ChromaLogger.LogLevel = (ChromaLogger.Level)(int)value; ChromaConfig.SetInt("Logger", "loggerLevel", (int)value); };
            ctLogSettings.FormatValue += delegate (float value) { return ((ChromaLogger.Level)(int)value).ToString(); };

            StringViewController ctPassword = ctSettings.AddString("Secrets", "What could it mean?!?!");
            ctPassword.GetValue += delegate { return ""; };
            ctPassword.SetValue += delegate (string value) {
                if (value.ToUpper() == "SAFETYHAZARD") {
                    ChromaConfig.WaiverRead = true;
                    AudioUtil.Instance.PlayOneShotSound("NightmareMode.wav");
                } else if (value.ToUpper() == "CREDITS") {
                    AudioUtil.Instance.PlayOneShotSound("ConfigReload.wav");
                }
            };

            SettingsMenuCreatedEvent?.Invoke(ctSettings);

            ChromaLogger.Log("Sub-menus " + (ChromaConfig.HideSubMenus ? "are" : "are not") + " hidden.");

            /*
             * SUB-MENUS
             */
            if (!ChromaConfig.HideSubMenus) {

                float[] presets = new float[colourPresets.Count];
                for (int i = 0; i < colourPresets.Count; i++) presets[i] = i;
                
                /*
                 * NOTES COLOURS
                 */
                SubMenu ctNotes = SettingsUI.CreateSubMenu("Chroma Notes");

                //A
                ListViewController ctAColour = ctNotes.AddList("Left Notes", presets);
                ctAColour.applyImmediately = true;
                ctAColour.GetValue += delegate {
                    String name = ChromaConfig.GetString("Notes", "colourA", "DEFAULT");
                    for (int i = 0; i < colourPresets.Count; i++) {
                        if (colourPresets[i].name == name) return i;
                    }
                    return 0;
                };
                ctAColour.SetValue += delegate (float value) {
                    ColourManager.A = colourPresets[(int)value].color;
                    ChromaConfig.SetString("Notes", "colourA", colourPresets[(int)value].name);
                };
                ctAColour.FormatValue += delegate (float value) {
                    return colourPresets[(int)value].name;
                };

                //B
                ListViewController ctBColour = ctNotes.AddList("Right Notes", presets);
                ctBColour.applyImmediately = true;
                ctBColour.GetValue += delegate {
                    String name = ChromaConfig.GetString("Notes", "colourB", "DEFAULT");
                    for (int i = 0; i < colourPresets.Count; i++) {
                        if (colourPresets[i].name == name) return i;
                    }
                    return 0;
                };
                ctBColour.SetValue += delegate (float value) {
                    ColourManager.B = colourPresets[(int)value].color;
                    ChromaConfig.SetString("Notes", "colourB", colourPresets[(int)value].name);
                };
                ctBColour.FormatValue += delegate (float value) {
                    return colourPresets[(int)value].name;
                };

                SettingsNoteMenuAddedEvent?.Invoke(ctNotes, presets, colourPresets);

                /*
                 * LIGHTS COLOURS
                 */
                SubMenu ctLights = SettingsUI.CreateSubMenu("Chroma Lights");

                ListViewController ctLightAmbientColour = ctLights.AddList("Ambient (bg) Lights", presets);
                ctLightAmbientColour.applyImmediately = true;
                ctLightAmbientColour.GetValue += delegate {
                    String name = ChromaConfig.GetString("Lights", "lightAmbient", "DEFAULT");
                    for (int i = 0; i < colourPresets.Count; i++) {
                        if (colourPresets[i].name == name) return i;
                    }
                    return 0;
                };
                ctLightAmbientColour.SetValue += delegate (float value) {
                    ColourManager.LightAmbient = colourPresets[(int)value].color;
                    ChromaConfig.SetString("Lights", "lightAmbient", colourPresets[(int)value].name);
                    ColourManager.RecolourAmbientLights(ColourManager.LightAmbient);
                };
                ctLightAmbientColour.FormatValue += delegate (float value) {
                    return colourPresets[(int)value].name;
                };

                //LightA
                ListViewController ctLightAColour = ctLights.AddList("Warm (red) Lights", presets);
                ctLightAColour.applyImmediately = true;
                ctLightAColour.GetValue += delegate {
                    String name = ChromaConfig.GetString("Lights", "lightColourA", "DEFAULT");
                    for (int i = 0; i < colourPresets.Count; i++) {
                        if (colourPresets[i].name == name) return i;
                    }
                    return 0;
                };
                ctLightAColour.SetValue += delegate (float value) {
                    ColourManager.LightA = colourPresets[(int)value].color;
                    ChromaConfig.SetString("Lights", "lightColourA", colourPresets[(int)value].name);
                };
                ctLightAColour.FormatValue += delegate (float value) {
                    return colourPresets[(int)value].name;
                };

                //LightB
                ListViewController ctLightBColour = ctLights.AddList("Cold (blue) Lights", presets);
                ctLightBColour.applyImmediately = true;
                ctLightBColour.GetValue += delegate {
                    String name = ChromaConfig.GetString("Lights", "lightColourB", "DEFAULT");
                    for (int i = 0; i < colourPresets.Count; i++) {
                        if (colourPresets[i].name == name) return i;
                    }
                    return 0;
                };
                ctLightBColour.SetValue += delegate (float value) {
                    ColourManager.LightB = colourPresets[(int)value].color;
                    ChromaConfig.SetString("Lights", "lightColourB", colourPresets[(int)value].name);
                };
                ctLightBColour.FormatValue += delegate (float value) {
                    return colourPresets[(int)value].name;
                };

                SettingsLightsMenuAddedEvent?.Invoke(ctLights, presets, colourPresets);

                /*
                 * OTHERS COLOURS
                 */
                SubMenu ctOthers = SettingsUI.CreateSubMenu("Chroma Aesthetics");

                //Barriers
                ListViewController ctBarrier = ctOthers.AddList("Barriers", presets);
                ctBarrier.applyImmediately = true;
                ctBarrier.GetValue += delegate {
                    String name = ChromaConfig.GetString("Aesthetics", "barrierColour", "Barrier Red");
                    for (int i = 0; i < colourPresets.Count; i++) {
                        if (colourPresets[i].name == name) return i;
                    }
                    return 0;
                };
                ctBarrier.SetValue += delegate (float value) {
                    ColourManager.BarrierColour = colourPresets[(int)value].color;
                    ChromaConfig.SetString("Aesthetics", "barrierColour", colourPresets[(int)value].name);
                };
                ctBarrier.FormatValue += delegate (float value) {
                    return colourPresets[(int)value].name;
                };

                //BarrierCorrection
                ListViewController ctBarrierCorrection = ctOthers.AddList("Barrier Col. Correction", new float[] { 0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f });
                ctBarrierCorrection.GetValue += delegate {
                    return ColourManager.barrierColourCorrectionScale;
                };
                ctBarrierCorrection.SetValue += delegate (float value) {
                    ColourManager.barrierColourCorrectionScale = value;
                    ChromaConfig.SetFloat("Aesthetics", "barrierColourCorrectionScale", value);
                };
                ctBarrierCorrection.FormatValue += delegate (float value) { return value * 100 + "%"; };

                //SignB
                ListViewController ctSignB = ctOthers.AddList("Neon Sign Top", presets);
                ctSignB.applyImmediately = true;
                ctSignB.GetValue += delegate {
                    String name = ChromaConfig.GetString("Aesthetics", "signColourB", "Notes Blue");
                    for (int i = 0; i < colourPresets.Count; i++) {
                        if (colourPresets[i].name == name) return i;
                    }
                    return 0;
                };
                ctSignB.SetValue += delegate (float value) {
                    ColourManager.SignB = colourPresets[(int)value].color;
                    ChromaConfig.SetString("Aesthetics", "signColourB", colourPresets[(int)value].name);
                    ColourManager.RecolourNeonSign(ColourManager.SignA, ColourManager.SignB);
                };
                ctSignB.FormatValue += delegate (float value) {
                    return colourPresets[(int)value].name;
                };

                //SignA
                ListViewController ctSignA = ctOthers.AddList("Neon Sign Bottom", presets);
                ctSignA.applyImmediately = true;
                ctSignA.GetValue += delegate {
                    String name = ChromaConfig.GetString("Aesthetics", "signColourA", "Notes Red");
                    for (int i = 0; i < colourPresets.Count; i++) {
                        if (colourPresets[i].name == name) return i;
                    }
                    return 0;
                };
                ctSignA.SetValue += delegate (float value) {
                    ColourManager.SignA = colourPresets[(int)value].color;
                    ChromaConfig.SetString("Aesthetics", "signColourA", colourPresets[(int)value].name);
                    ColourManager.RecolourNeonSign(ColourManager.SignA, ColourManager.SignB);
                };
                ctSignA.FormatValue += delegate (float value) {
                    return colourPresets[(int)value].name;
                };

                //LaserPointer
                ListViewController ctLaserColour = ctOthers.AddList("Laser Pointer", presets);
                ctLaserColour.applyImmediately = true;
                ctLaserColour.GetValue += delegate {
                    String name = ChromaConfig.GetString("Aesthetics", "laserPointerColour", "Notes Blue");
                    for (int i = 0; i < colourPresets.Count; i++) {
                        if (colourPresets[i].name == name) return i;
                    }
                    return 0;
                };
                ctLaserColour.SetValue += delegate (float value) {
                    ColourManager.LaserPointerColour = colourPresets[(int)value].color;
                    ChromaConfig.SetString("Aesthetics", "laserPointerColour", colourPresets[(int)value].name);
                    //ColourManager.RecolourLaserPointer(ColourManager.LaserPointerColour);
                    ColourManager.RecolourMenuStuff(ColourManager.A, ColourManager.B, ColourManager.LightA, ColourManager.LightB, ColourManager.Platform, ColourManager.LaserPointerColour);
                };
                ctLaserColour.FormatValue += delegate (float value) {
                    return colourPresets[(int)value].name;
                };

                ListViewController ctPlatform = ctOthers.AddList("Platform Accoutrements", presets);
                ctPlatform.applyImmediately = true;
                ctPlatform.GetValue += delegate {
                    String name = ChromaConfig.GetString("Aesthetics", "platformAccoutrements", "DEFAULT");
                    for (int i = 0; i < colourPresets.Count; i++) {
                        if (colourPresets[i].name == name) return i;
                    }
                    return 0;
                };
                ctPlatform.SetValue += delegate (float value) {
                    ColourManager.Platform = colourPresets[(int)value].color;
                    ChromaConfig.SetString("Aesthetics", "platformAccoutrements", colourPresets[(int)value].name);
                    ColourManager.RecolourMenuStuff(ColourManager.A, ColourManager.B, ColourManager.LightA, ColourManager.LightB, ColourManager.Platform, ColourManager.LaserPointerColour);
                };
                ctPlatform.FormatValue += delegate (float value) {
                    return colourPresets[(int)value].name;
                };

                SettingsOthersMenuAddedEvent?.Invoke(ctOthers, presets, colourPresets);

                ExtensionSubMenusEvent?.Invoke(presets, colourPresets);

            }

            GameplaySettingsUISetup();

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

        public delegate void GameplaySubMenuCreatedDelegate(string subMenuName);
        public static event GameplaySubMenuCreatedDelegate GameplaySubMenuCreatedEvent;

        public delegate void ExtensionGameplayMenusDelegate();
        public static event ExtensionGameplayMenusDelegate ExtensionGameplayMenusEvent;
        
        private static void GameplaySettingsUISetup() {
            
            /*
             * COLOURS
             */
            GameplaySettingsUI.CreateSubmenuOption(GameplaySettingsPanels.PlayerSettingsRight, "Chroma Colours", "MainMenu", "CTC", "Choose your Colour Scheme and more!");

            ToggleOption technicolourToggle = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.PlayerSettingsRight, "Technicolour", "CTC", "Enable/Disable Technicolour.  See Technicolour options below.");
            technicolourToggle.GetValue = ChromaConfig.TechnicolourEnabled;
            technicolourToggle.OnToggle += TechnicolourToggled;
            technicolourToggle.AddConflict("RNG PLights");
            
            /*
             * EVENTS
             */
            GameplaySettingsUI.CreateSubmenuOption(GameplaySettingsPanels.PlayerSettingsRight, "Chroma Events", "MainMenu", "CTE", "Toggle RGB lighting and special events");

            ToggleOption rgbLightsToggle = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.PlayerSettingsRight, "RGB Lights", "CTE", "Enable/Disable RGB lighting events.");
            rgbLightsToggle.GetValue = ChromaConfig.CustomColourEventsEnabled;
            rgbLightsToggle.OnToggle += RGBEventsToggled;
            rgbLightsToggle.AddConflict("Darth Maul");

            ToggleOption specialEventsToggle = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.PlayerSettingsRight, "Special Events", "CTE", "Enable/Disable Special Events, such as note size changing, player heal/harm events, and rotation events.");
            specialEventsToggle.GetValue = ChromaConfig.CustomSpecialEventsEnabled;
            specialEventsToggle.OnToggle += SpecialEventsToggled;
            specialEventsToggle.AddConflict("Darth Maul");




            GameplaySettingsUI.CreateSubmenuOption(GameplaySettingsPanels.PlayerSettingsRight, "Techni. Options", "CTC", "CTT", "Adjust Technicolour Settings.");

            List<Tuple<float, string>> technicolourOptions = new List<Tuple<float, string>> {
                { 0f, "OFF" },
                { 1f, "WARM/COLD" },
                { 2f, "EITHER" },
                { 3f, "TRUE RANDOM" }
            };

            //float[] techniOptions = new float[technicolourOptions.Count];
            //for (int i = 0; i < technicolourOptions.Count; i++) techniOptions[i] = i;

            MultiSelectOption techniLights = GameplaySettingsUI.CreateListOption(GameplaySettingsPanels.PlayerSettingsRight, "Tech. Lights", "CTT", "Technicolour style of the lights.");
            for (int i = 0; i < technicolourOptions.Count; i++) techniLights.AddOption(i, technicolourOptions[i].Item2);
            techniLights.GetValue += delegate {
                return (int)ChromaConfig.TechnicolourLightsStyle;
            };
            techniLights.OnChange += delegate (float value) {
                ColourManager.TechnicolourStyle style = ColourManager.GetTechnicolourStyleFromFloat(value);
                ChromaConfig.TechnicolourLightsStyle = style;
            };

            MultiSelectOption techniLightsGrouping = GameplaySettingsUI.CreateListOption(GameplaySettingsPanels.PlayerSettingsRight, "Lights Grouping", "CTT", ChromaConfig.WaiverRead ? "The more isolated, the more intense.  Isolated Event has the best performance.\n  <color=red>Mayhem prevents fades and flashes from working properly</color>." : "Isolated Event for better performance, but more chaotic lighting");
            techniLightsGrouping.AddOption(0f, "Standard");
            techniLightsGrouping.AddOption(1f, "Isolated Event");
            if (ChromaConfig.WaiverRead) techniLightsGrouping.AddOption(2f, "Isolated (Mayhem)");
            techniLightsGrouping.GetValue += delegate {
                return (int)ChromaConfig.TechnicolourLightsGrouping;
            };
            techniLightsGrouping.OnChange += delegate (float value) {
                ChromaConfig.TechnicolourLightsGrouping = ColourManager.GetTechnicolourLightsGroupingFromFloat(value);
            };

            MultiSelectOption techniFrequency = GameplaySettingsUI.CreateListOption(GameplaySettingsPanels.PlayerSettingsRight, "Lights Freq", "CTT", "The higher the frequency, the more colour changes.  10% is default.");
            for (int i = 1; i <= 20; i++) techniFrequency.AddOption(0.05f * i, i == 2 ? "10% (Def)" : (5f * i) + "%");
            techniFrequency.GetValue += delegate {
                return ChromaConfig.TechnicolourLightsFrequency;
            };
            techniFrequency.OnChange += delegate (float value) {
                ChromaConfig.TechnicolourLightsFrequency = value;
            };

            /*ToggleOption techniIndividualLights = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.PlayerSettingsRight, "Isolated Lights", "CTT", "If enabled, Technicolour will only affect one light source at a time.  This results in much, much more colour variety being possible, but also can look excessively chaotic.");
            techniIndividualLights.GetValue = ChromaConfig.TechnicolourLightsIndividual;
            techniIndividualLights.OnToggle += delegate (bool value) {
                ChromaConfig.TechnicolourLightsIndividual = value;
            };*/

            /*MultiSelectOption techniBarriers = GameplaySettingsUI.CreateListOption(GameplaySettingsPanels.PlayerSettingsRight, "Tech. Walls", "CTT", "Technicolour style of the walls/barriers.");
            for (int i = 0; i < technicolourOptions.Count; i++) techniBarriers.AddOption(i, technicolourOptions[i].Item2);
            techniBarriers.GetValue += delegate {
                return (int)ChromaConfig.TechnicolourWallsStyle;
            };
            techniBarriers.OnChange += delegate (float value) {
                ColourManager.TechnicolourStyle style = ColourManager.GetTechnicolourStyleFromFloat(value);
                ChromaConfig.TechnicolourWallsStyle = style;
            };*/

            //Walls don't need to have other options since they only work nicely with Either
            ToggleOption techniWalls = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.PlayerSettingsRight, "Tech. Barriers", "CTT", "If enabled, Barriers will rainbowify!");
            techniWalls.GetValue = ChromaConfig.TechnicolourWallsStyle == ColourManager.TechnicolourStyle.ANY_PALETTE;
            techniWalls.OnToggle += delegate (bool value) {
                ChromaConfig.TechnicolourWallsStyle = value ? ColourManager.TechnicolourStyle.ANY_PALETTE : ColourManager.TechnicolourStyle.OFF;
            };


            MultiSelectOption techniBlocks = GameplaySettingsUI.CreateListOption(GameplaySettingsPanels.PlayerSettingsRight, "Tech. Blocks", "CTT", "Technicolour style of the blocks.");
            for (int i = 0; i < technicolourOptions.Count; i++) techniBlocks.AddOption(i, technicolourOptions[i].Item2);
            techniBlocks.GetValue += delegate {
                return (int)ChromaConfig.TechnicolourBlocksStyle;
            };
            techniBlocks.OnChange += delegate (float value) {
                ColourManager.TechnicolourStyle style = ColourManager.GetTechnicolourStyleFromFloat(value);
                ChromaConfig.TechnicolourBlocksStyle = style;
            };

            MultiSelectOption techniSsabers = GameplaySettingsUI.CreateListOption(GameplaySettingsPanels.PlayerSettingsRight, "Tech. Sabers", "CTT", "Technicolour style of the sabers.");
            for (int i = 0; i < technicolourOptions.Count; i++) techniSsabers.AddOption(i, technicolourOptions[i].Item2);
            techniSsabers.GetValue += delegate {
                return (int)ChromaConfig.TechnicolourSabersStyle;
            };
            techniSsabers.OnChange += delegate (float value) {
                ColourManager.TechnicolourStyle style = ColourManager.GetTechnicolourStyleFromFloat(value);
                ChromaConfig.TechnicolourSabersStyle = style;
            };

            ToggleOption techniSabersMismatch = GameplaySettingsUI.CreateToggleOption(GameplaySettingsPanels.PlayerSettingsRight, "Desync Sabers", "CTT", "If true, technicolour sabers will have their \"time\" start and progress differently, resulting in their colours not matching so often.");
            techniSabersMismatch.GetValue = !ChromaConfig.MatchTechnicolourSabers;
            techniSabersMismatch.OnToggle += TechnicolourSaberMismatchToggled;

            GameplaySubMenuCreatedEvent?.Invoke("CTC");
            GameplaySubMenuCreatedEvent?.Invoke("CTE");
            GameplaySubMenuCreatedEvent?.Invoke("CTT");

            ExtensionGameplayMenusEvent?.Invoke();
        }

        private static void TechnicolourToggled(bool b) {
            ChromaConfig.TechnicolourEnabled = b;
        }

        private static void RGBEventsToggled(bool b) {
            ChromaConfig.CustomColourEventsEnabled = b;
        }

        private static void SpecialEventsToggled(bool b) {
            ChromaConfig.CustomSpecialEventsEnabled = b;
        }

        private static void TechnicolourSaberMismatchToggled(bool b) {
            ChromaConfig.MatchTechnicolourSabers = !b;
        }


        private static void InitializePresetList() {

            colourPresets = new List<NamedColor>();// new List<Tuple<string, Color>>();

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
                new NamedColor( "DEFAULT", Color.clear ),

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
