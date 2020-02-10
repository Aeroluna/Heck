using Chroma.Settings;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using Chroma.Utils;
using Chroma.Beatmap.Events;
using UnityEngine;
using Chroma.Misc;
using System.IO;
using IPA.Loader;
using IPA;
using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.GameplaySetup;

namespace Chroma {

    public class ChromaPlugin {

        private static Version assemblyVersion = typeof(ChromaPlugin).Assembly.GetName().Version;
        public static string Version = assemblyVersion.Major+"."+assemblyVersion.Minor+"."+assemblyVersion.Build;

        private static ChromaPlugin _instance;
        /// <summary>
        /// Creates a new Instance if one does not exist.
        /// </summary>
        public static ChromaPlugin Instance {
            get {
                return _instance;
            }
        }

        internal static ChromaPlugin Instantiate(Plugin plugin) {
            _instance = new ChromaPlugin();
            _instance.plugin = plugin;
            _instance.Initialize();
            return _instance;
        }

        /// <summary>
        /// Called when the game transitions to the Main Menu from any other scene.
        /// </summary>
        public static event MainMenuLoadedDelegate MainMenuLoadedEvent;
        public delegate void MainMenuLoadedDelegate();

        /// <summary>
        /// Called when the player starts a song.
        /// </summary>
        public static event SongSceneLoadedDelegate SongSceneLoadedEvent;
        public delegate void SongSceneLoadedDelegate();

        /// <summary>
        /// The IllusionPlugin holding ChromaPlugin
        /// </summary>
        public Plugin plugin;

        private ChromaPlugin() { }

        HarmonyInstance coreHarmony = HarmonyInstance.Create("net.binaryelement.chroma");
        List<HarmonyInstance> harmonyInstances = new List<HarmonyInstance>();

        /// <summary>
        /// Returns the Harmony instance the core Chroma plugin uses.
        /// </summary>
        public HarmonyInstance CoreHarmonyInstance {
            get { return coreHarmony; }
        }

        /// <summary>
        /// Returns a readonly list of all HarmonyInstances used by Chroma and its extensions, including the CoreHarmonyInstance
        /// </summary>
        public List<HarmonyInstance> HarmonyInstances {
            get { return new List<HarmonyInstance>(harmonyInstances); }
        }

        List<IChromaExtension> chromaExtensions = new List<IChromaExtension>();

        /// <summary>
        /// Returns a readonly list of all Chroma extensions detected and registered on startup
        /// </summary>
        public List<IChromaExtension> ChromaExtensions {
            get { return new List<IChromaExtension>(chromaExtensions); }
        }

        private void Initialize() {

            try {

                try {
                    Directory.CreateDirectory(Environment.CurrentDirectory.Replace('\\', '/') + "/UserData/Chroma");
                } catch (Exception e) {
                    ChromaLogger.Log("Error " + e.Message + " while trying to create Chroma directory", ChromaLogger.Level.WARNING);
                }

                ChromaLogger.Log("************************************", ChromaLogger.Level.INFO);
                ChromaLogger.Log("Initializing Chroma [" + Version + "]", ChromaLogger.Level.INFO);
                ChromaLogger.Log("************************************", ChromaLogger.Level.INFO);

                //Used for getting gamemode data mostly
                try {
                    ChromaLogger.Log("Initializing Coordinators");
                    BaseGameMode.InitializeCoordinators();
                } catch (Exception e) {
                    ChromaLogger.Log("Error initializing coordinators", ChromaLogger.Level.ERROR);
                    throw e;
                }

                ChromaLogger.Log("Registering scenechange events");
                SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
                SceneManager.sceneLoaded += SceneManager_sceneLoaded;

                //Getting and starting all the extension plugins
                try {
                    ChromaLogger.Log("Checking for extensions.");
                    foreach (PluginLoader.PluginInfo pluginInfo in PluginManager.AllPlugins) {
                        //We can't get IBeatSaberPlugin references
                        if (plugin is IChromaExtension chromaExtension) {
                            chromaExtension.ChromaApplicationStarted(this);
                            chromaExtensions.Add(chromaExtension);
                        }
                    }
                } catch (Exception) {
                    ChromaLogger.Log("Error adding all Extension plugins!  Extension registration interrupted.", ChromaLogger.Level.ERROR);
                }

                //Harmony & extension Harmony patches
                ChromaLogger.Log("Patching with Harmony.");
                try {
                    coreHarmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
                    harmonyInstances.Add(coreHarmony);
                    foreach (IChromaExtension extension in chromaExtensions) {
                        HarmonyInstance newPatch = extension.PatchHarmony();
                        if (newPatch != null) harmonyInstances.Add(newPatch);
                    }
                } catch (Exception e) {
                    ChromaLogger.Log(e);
                }

                ChromaLogger.Log("Creating AudioUtil");
                AudioUtil ab = AudioUtil.Instance;

                //Configuration Files
                try {
                    ChromaLogger.Log("Initializing Configuration");
                    ChromaConfig.Init();
                    ChromaConfig.LoadSettings(ChromaConfig.LoadSettingsType.INITIAL);
                    GameplaySetup.instance.AddTab("Chroma", "Chroma.Settings.modifiers.bsml", ChromaSettingsUI.instance);
                } catch (Exception e) {
                    ChromaLogger.Log("Error loading Chroma configuration", ChromaLogger.Level.ERROR);
                    throw e;
                }

                //Side panel
                try {
                    ChromaLogger.Log("Stealing Patch Notes Panel");
                    Greetings.RegisterChromaSideMenu();
                    SidePanelUtil.ReleaseInfoEnabledEvent += ReleaseInfoEnabled;
                } catch (Exception e) {
                    ChromaLogger.Log("Error handling UI side panel", ChromaLogger.Level.ERROR);
                    throw e;
                }

            } catch (Exception e) {
                ChromaLogger.Log("Failed to initialize ChromaPlugin!  Major error!", ChromaLogger.Level.ERROR);
                ChromaLogger.Log(e);
            }

            ChromaLogger.Log("Chroma finished initializing.  " + chromaExtensions.Count + " extensions found.", ChromaLogger.Level.INFO);

            ChromaUtils.SetSongCoreCapability("Chroma");
            ChromaUtils.SetSongCoreCapability("ChromaLite");
        }

        private void SceneManagerOnActiveSceneChanged(Scene current, Scene next) {
            ChromaLogger.Log("Scene change " + current.name + " -> " + next.name, ChromaLogger.Level.DEBUG);

            if (current.name == "GameCore") {
                if (next.name != "GameCore") {
                    Time.timeScale = 1f;
                    //ChromaBehaviour.ClearInstance();
                    //if (ChromaConfig.reloadGameModes) ChromaGameMode.InitializeGameModes();
                    //ChromaConfig.LoadSettings(ChromaConfig.LoadSettingsType.MENU_LOADED);
                    MainMenuLoadedEvent?.Invoke();
                }
            } else {
                if (next.name == "GameCore") {
                    //overrideGameMode = null;
                    //ChromaBehaviour.CreateNewInstance(GetSelectedGameMode());
                    ChromaBehaviour.CreateNewInstance();
                    SongSceneLoadedEvent?.Invoke();
                }
            }

        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode) {
            if (scene.name == "MenuViewControllers") {
                ChromaSettingsUI.InitializeMenu();
                BSMLSettings.instance.AddSettingsMenu("Chroma", "Chroma.Settings.settings.bsml", ChromaSettingsUI.instance);
            }
        }
        
        public void ReleaseInfoEnabled() {
            SidePanelUtil.SetPanel(ChromaSettingsUI.floatToPanel((float)ChromaConfig.SidePanel));
            //SidePanelUtil.SetPanel("chroma");
            //SidePanelUtil.SetPanelDirectly(SafetyWaiver.GetSafetyWaiverUIMessage());
        }

        public void OnUpdate() {

            if (Input.GetKeyDown(KeyCode.Backslash)) {
                ChromaConfig.LoadSettings(ChromaConfig.LoadSettingsType.MANUAL);
            }

            if (Input.GetKeyDown(KeyCode.Period) && ChromaConfig.DebugMode) {

                if (Input.GetKey(KeyCode.Alpha1)) ColourManager.RecolourNeonSign((Color)ColourManager.SignA, (Color)ColourManager.SignB);
                else if (Input.GetKey(KeyCode.Alpha2)) ColourManager.RefreshLights();
                else {

                    ChromaLogger.Log(" [[ Debug Info ]]");

                    if (ChromaConfig.TechnicolourEnabled) {
                        ChromaLogger.Log("TechnicolourStyles (Lights | Walls | Notes | Sabers) : " + ChromaConfig.TechnicolourLightsStyle + " | " + ChromaConfig.TechnicolourWallsStyle + " | " + ChromaConfig.TechnicolourBlocksStyle + " | " + ChromaConfig.TechnicolourSabersStyle);
                        ChromaLogger.Log("Technicolour (Lights | Walls | Notes | Sabers) : " + ColourManager.TechnicolourLights + " | " + ColourManager.TechnicolourBarriers + " | " + ColourManager.TechnicolourBlocks + " | " + ColourManager.TechnicolourSabers);
                    }

                    DebugButtonPressedEvent?.Invoke();

                }

            }
        }

        public delegate void DebugButtonPressedDelegate();
        public static DebugButtonPressedDelegate DebugButtonPressedEvent;

    }
    
}
