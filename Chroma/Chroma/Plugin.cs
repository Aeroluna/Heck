using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Settings;
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

            ChromaLogger.Log("Initializing Chroma [" + Version + "]", ChromaLogger.Level.INFO);

            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Harmony patches
            var harmony = new Harmony("net.binaryelement.chroma");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            // Configuration Files
            ChromaConfig.Init();
            ChromaConfig.LoadSettings(ChromaConfig.LoadSettingsType.INITIAL);
            GameplaySetup.instance.AddTab("Chroma", "Chroma.Settings.modifiers.bsml", ChromaSettingsUI.instance);
            if (ChromaConfig.LightshowMenu) GameplaySetup.instance.AddTab("Lightshow Modifiers", "Chroma.Settings.lightshow.bsml", ChromaSettingsUI.instance);

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events");

            // Side panel
            Greetings.RegisterChromaSideMenu();
            SidePanelUtil.ReleaseInfoEnabledEvent += ReleaseInfoEnabled;
        }

        private void ReleaseInfoEnabled()
        {
            SidePanelUtil.SetPanel(Enum.GetName(typeof(ChromaConfig.SidePanelEnum), ChromaConfig.SidePanel));
        }

        [Init]
        public void Init(IPALogger pluginLogger)
        {
            ChromaLogger.logger = pluginLogger;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            if (scene.name == "MenuViewControllers") BSMLSettings.instance.AddSettingsMenu("Chroma", "Chroma.Settings.settings.bsml", ChromaSettingsUI.instance);
        }

        public void OnActiveSceneChanged(Scene current, Scene next)
        {
            if (current.name == "GameCore")
            {
                if (next.name != "GameCore")
                {
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