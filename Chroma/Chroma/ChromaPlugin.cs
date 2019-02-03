using Chroma.Settings;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IllusionPlugin;
using IllusionInjector;
using Harmony;
using Chroma.Utils;
using Chroma.Beatmap.Events;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Chroma.Misc;
using System.IO;

namespace Chroma {

    public class ChromaPlugin {

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

                ChromaLogger.Init();

                ChromaLogger.Log("************************************", ChromaLogger.Level.INFO);
                ChromaLogger.Log("Initializing Chroma [" + plugin.Version + "]", ChromaLogger.Level.INFO);
                ChromaLogger.Log("************************************", ChromaLogger.Level.INFO);

                //Used for getting gamemode data mostly
                try {
                    BaseGameMode.InitializeCoordinators();
                } catch (Exception e) {
                    ChromaLogger.Log("Error initializing coordinators", ChromaLogger.Level.ERROR);
                    throw e;
                }

                SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
                SceneManager.sceneLoaded += SceneManager_sceneLoaded;

                //Getting and starting all the extension plugins
                try {
                    foreach (IPlugin plugin in PluginManager.Plugins) {
                        if (plugin is IChromaExtension chromaExtension) {
                            chromaExtension.ChromaApplicationStarted(this);
                            chromaExtensions.Add(chromaExtension);
                        }
                    }
                } catch (Exception e) {
                    ChromaLogger.Log("Error adding all Extension plugins", ChromaLogger.Level.ERROR);
                    throw e;
                }

                //Harmony & extension Harmony patches
                try {
                    coreHarmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
                    harmonyInstances.Add(coreHarmony);
                    foreach (IChromaExtension extension in chromaExtensions) {
                        HarmonyInstance newPatch = extension.PatchHarmony();
                        if (newPatch != null) harmonyInstances.Add(newPatch);
                    }
                } catch (Exception e) {
                    ChromaLogger.Log(e);
                    ChromaLogger.Log("This plugin requires Harmony.  Either you do not have it installed, or there was an error.", ChromaLogger.Level.ERROR);
                }

                AudioUtil ab = AudioUtil.Instance;

                //Configuration Files
                try {
                    ChromaConfig.Init();
                    ChromaConfig.LoadSettings(ChromaConfig.LoadSettingsType.INITIAL);
                } catch (Exception e) {
                    ChromaLogger.Log("Error loading Chroma configuration", ChromaLogger.Level.ERROR);
                    throw e;
                }
                
                ColourManager.RefreshLights();

                //Side panel
                try {
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

            try {
                typeof(SongLoaderPlugin.SongLoader).InvokeMethod("RegisterCapability", new object[] { "Chroma Lighting Events" });
                //typeof(SongLoaderPlugin.SongLoader).InvokeMethod("RegisterCapability", new object[] { "Chroma Gamemodes" });
            } catch (Exception) {
                // This version of SongLoader doesn't support capabilities
            }
        }

        private void SceneManagerOnActiveSceneChanged(Scene current, Scene next) {
            ChromaLogger.Log("Scene change " + current.name + " -> " + next.name, ChromaLogger.Level.DEBUG);
            doRefreshLights = true;

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
            if (scene.name == "Menu") {
                try {
                    ChromaSettingsUI.InitializeMenu();

                } catch (Exception e) {
                    ChromaLogger.Log(e);
                }
            }
        }

        public static string infoKey = "chroma";
        public void ReleaseInfoEnabled() {
            if (infoKey == "chroma" && (UnityEngine.Random.value < 0.025f || Input.GetKey(KeyCode.Alpha5))) {
                string[] split = Greetings.GetGreeting(ChromaConfig.UserID, ChromaConfig.Username);
                string message = "";
                foreach (string s in split) {
                    message = message + s + Environment.NewLine;
                }
                SidePanelUtil.SetPanelDirectly(message);
            } else {
                SidePanelUtil.SetPanel(infoKey);
                infoKey = "chroma";
            }
            //SidePanelUtil.SetPanelDirectly(SafetyWaiver.GetSafetyWaiverUIMessage());
        }

        /// <summary>
        /// Workaround for lights not being loaded in time for scene load methods
        /// </summary>
        public bool doRefreshLights = false;

        public void OnUpdate() {
            if (doRefreshLights && SceneManager.GetActiveScene().name == "Menu") {
                ColourManager.RefreshLights();
                doRefreshLights = false;
            }

            if (Input.GetKeyDown(KeyCode.Backslash)) {
                ChromaConfig.LoadSettings(ChromaConfig.LoadSettingsType.MANUAL);
            }

            if (Input.GetKeyDown(KeyCode.Period) && ChromaConfig.DebugMode) {

                if (Input.GetKey(KeyCode.Alpha1)) ColourManager.RecolourNeonSign(ColourManager.SignA, ColourManager.SignB);
                else if (Input.GetKey(KeyCode.Alpha2)) ColourManager.RefreshLights();
                else if (Input.GetKey(KeyCode.Alpha3)) Test();
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
        
        public void Test() {
            SidePanelUtil.SetPanel("chromaCredits");
        }

        public delegate void DebugButtonPressedDelegate();
        public static DebugButtonPressedDelegate DebugButtonPressedEvent;

    }
    
}
