using BeatSaberMarkupLanguage.GameplaySetup;
using Chroma.Misc;
using Chroma.Settings;
using Chroma.Utils;
using HarmonyLib;
using IPA;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace Chroma
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    internal class Plugin
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

        [OnStart]
        public void OnApplicationStart()
        {
            Directory.CreateDirectory(Environment.CurrentDirectory.Replace('\\', '/') + "/UserData/Chroma");

            ChromaLogger.Log("************************************", ChromaLogger.Level.INFO);
            ChromaLogger.Log("Initializing Chroma [" + Version + "]", ChromaLogger.Level.INFO);
            ChromaLogger.Log("************************************", ChromaLogger.Level.INFO);

            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;

            //Harmony patches
            var harmony = new Harmony("net.binaryelement.chroma");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //Configuration Files
            ChromaLogger.Log("Initializing Configuration");
            ChromaConfig.Init();
            ChromaConfig.LoadSettings(ChromaConfig.LoadSettingsType.INITIAL);
            GameplaySetup.instance.AddTab("Chroma", "Chroma.Settings.modifiers.bsml", ChromaSettingsUI.instance);
            if (ChromaConfig.LightshowMenu) GameplaySetup.instance.AddTab("Lightshow Modifiers", "Chroma.Settings.lightshow.bsml", ChromaSettingsUI.instance);

            //Side panel
            ChromaLogger.Log("Stealing Patch Notes Panel");
            Greetings.RegisterChromaSideMenu();
            SidePanelUtil.ReleaseInfoEnabledEvent += ReleaseInfoEnabled;

            ChromaLogger.Log("Chroma finished initializing.");
        }

        private void ReleaseInfoEnabled()
        {
            SidePanelUtil.SetPanel(ChromaSettingsUI.floatToPanel((float)ChromaConfig.SidePanel));
        }

        [Init]
        public void Init(IPALogger pluginLogger)
        {
            ChromaLogger.logger = pluginLogger;
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
    }
}