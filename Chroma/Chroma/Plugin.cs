using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Settings;
using Chroma.Misc;
using Chroma.Settings;
using Chroma.Utils;
using Harmony;
using IPA;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace Chroma
{
    public class Plugin : IBeatSaberPlugin
    {
        private static Version assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
        internal static string Version = assemblyVersion.Major + "." + assemblyVersion.Minor + "." + assemblyVersion.Build;

        internal const string REQUIREMENT_NAME = "Chroma";

        /// <summary>
        /// Called when the game transitions to the Main Menu from any other scene.
        /// </summary>
        internal static event MainMenuLoadedDelegate MainMenuLoadedEvent;

        internal delegate void MainMenuLoadedDelegate();

        /// <summary>
        /// Called when the player starts a song.
        /// </summary>
        internal static event SongSceneLoadedDelegate SongSceneLoadedEvent;

        internal delegate void SongSceneLoadedDelegate();

        public void OnApplicationStart()
        {
            try
            {
                try
                {
                    Directory.CreateDirectory(Environment.CurrentDirectory.Replace('\\', '/') + "/UserData/Chroma");
                }
                catch (Exception e)
                {
                    ChromaLogger.Log("Error " + e.Message + " while trying to create Chroma directory", ChromaLogger.Level.WARNING);
                }

                ChromaLogger.Log("************************************", ChromaLogger.Level.INFO);
                ChromaLogger.Log("Initializing Chroma [" + Version + "]", ChromaLogger.Level.INFO);
                ChromaLogger.Log("************************************", ChromaLogger.Level.INFO);

                //Harmony patches
                var harmony = HarmonyInstance.Create("net.binaryelement.chroma");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                //Configuration Files
                try
                {
                    ChromaLogger.Log("Initializing Configuration");
                    ChromaConfig.Init();
                    ChromaConfig.LoadSettings(ChromaConfig.LoadSettingsType.INITIAL);
                    GameplaySetup.instance.AddTab("Chroma", "Chroma.Settings.modifiers.bsml", ChromaSettingsUI.instance);
                    if (ChromaConfig.LightshowMenu) GameplaySetup.instance.AddTab("Lightshow Modifiers", "Chroma.Settings.lightshow.bsml", ChromaSettingsUI.instance);
                }
                catch (Exception e)
                {
                    ChromaLogger.Log("Error loading Chroma configuration", ChromaLogger.Level.ERROR);
                    throw e;
                }

                //Side panel
                try
                {
                    ChromaLogger.Log("Stealing Patch Notes Panel");
                    Greetings.RegisterChromaSideMenu();
                    SidePanelUtil.ReleaseInfoEnabledEvent += ReleaseInfoEnabled;
                }
                catch (Exception e)
                {
                    ChromaLogger.Log("Error handling UI side panel", ChromaLogger.Level.ERROR);
                    throw e;
                }
            }
            catch (Exception e)
            {
                ChromaLogger.Log("Failed to initialize ChromaPlugin!  Major error!", ChromaLogger.Level.ERROR);
                ChromaLogger.Log(e);
            }

            ChromaLogger.Log("Chroma finished initializing.");
        }

        private void ReleaseInfoEnabled()
        {
            SidePanelUtil.SetPanel(ChromaSettingsUI.floatToPanel((float)ChromaConfig.SidePanel));
        }

        public void Init(object thisIsNull, IPALogger pluginLogger)
        {
            ChromaLogger.logger = pluginLogger;
        }

        public void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Backslash))
            {
                ChromaConfig.LoadSettings(ChromaConfig.LoadSettingsType.MANUAL);
            }

            if (Input.GetKeyDown(KeyCode.Period) && ChromaConfig.DebugMode)
            {
                ChromaLogger.Log(" [[ Debug Info ]]");

                if (ChromaConfig.TechnicolourEnabled)
                {
                    ChromaLogger.Log("TechnicolourStyles (Lights | Walls | Notes | Sabers) : " + ChromaConfig.TechnicolourLightsStyle + " | " + ChromaConfig.TechnicolourWallsStyle + " | " + ChromaConfig.TechnicolourBlocksStyle + " | " + ChromaConfig.TechnicolourSabersStyle);
                    ChromaLogger.Log("Technicolour (Lights | Walls | Notes | Sabers) : " + ColourManager.TechnicolourLights + " | " + ColourManager.TechnicolourBarriers + " | " + ColourManager.TechnicolourBlocks + " | " + ColourManager.TechnicolourSabers);
                }
            }
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            if (scene.name == "MenuViewControllers")
            {
                ChromaSettingsUI.InitializeMenu();
            }
        }

        public void OnActiveSceneChanged(Scene current, Scene next)
        {
            if (current.name == "GameCore")
            {
                if (next.name != "GameCore")
                {
                    Time.timeScale = 1f;
                    MainMenuLoadedEvent?.Invoke();
                }
            }
            else
            {
                if (next.name == "GameCore")
                {
                    ChromaBehaviour.CreateNewInstance();
                    SongSceneLoadedEvent?.Invoke();
                }
            }
        }

        public void OnFixedUpdate()
        {
        }

        public void OnApplicationQuit()
        {
        }

        public void OnSceneUnloaded(Scene scene)
        {
        }
    }
}