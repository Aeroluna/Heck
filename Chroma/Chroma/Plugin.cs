namespace Chroma
{
    using System;
    using System.Reflection;
    using BeatSaberMarkupLanguage.GameplaySetup;
    using Chroma.Events;
    using Chroma.Settings;
    using Chroma.Utils;
    using HarmonyLib;
    using IPA;
    using IPA.Config;
    using IPA.Config.Stores;
    using UnityEngine.SceneManagement;
    using static Chroma.ChromaColorManager;
    using IPALogger = IPA.Logging.Logger;

    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        internal const string REQUIREMENT_NAME = "Chroma";
        internal const string HARMONYID = "com.noodle.BeatSaber.Chroma";

        internal static readonly Harmony HarmonyInstance = new Harmony(HARMONYID);

        internal static event Action MainMenuLoadedEvent;

        internal static event Action SongSceneLoadedEvent;

        [Init]
        public void Init(IPALogger pluginLogger, Config conf)
        {
            ChromaLogger.IPAlogger = pluginLogger;
            ChromaConfig.Instance = conf.Generated<ChromaConfig>();
        }

        [OnEnable]
        public void OnEnable()
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            // Harmony patches
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            // Configuration Files
            MainMenuLoadedEvent += OnMainMenuLoaded;
            SongSceneLoadedEvent += OnSongLoaded;

            MainMenuLoadedEvent += CleanupSongEvents;
            SongSceneLoadedEvent += CleanupSongEvents;

            GameplaySetup.instance.AddTab("Chroma", "Chroma.Settings.modifiers.bsml", ChromaSettingsUI.instance);
            if (ChromaConfig.Instance.LightshowMenu)
            {
                GameplaySetup.instance.AddTab("Lightshow Modifiers", "Chroma.Settings.lightshow.bsml", ChromaSettingsUI.instance);
            }

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events");
        }

        [OnDisable]
        public void OnDisable()
        {
            // Harmony patches
            HarmonyInstance.UnpatchAll(HARMONYID);

            // Configuration Files
            MainMenuLoadedEvent -= OnMainMenuLoaded;
            SongSceneLoadedEvent -= OnSongLoaded;

            MainMenuLoadedEvent -= CleanupSongEvents;
            SongSceneLoadedEvent -= CleanupSongEvents;

            GameplaySetup.instance.RemoveTab("Chroma");
            GameplaySetup.instance.RemoveTab("Lightshow Modifiers");

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events", false);
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

        private static void OnMainMenuLoaded()
        {
            RemoveNoteTypeColourOverride(NoteType.NoteA);
            RemoveNoteTypeColourOverride(NoteType.NoteB);
        }

        private static void OnSongLoaded()
        {
            RemoveNoteTypeColourOverride(NoteType.NoteA);
            RemoveNoteTypeColourOverride(NoteType.NoteB);
        }

        private static void CleanupSongEvents()
        {
            ChromaNoteColourEvent.SavedNoteColours.Clear();
            ChromaLightColourEvent.LightColours.Clear();
            ChromaGradientEvent.Gradients.Clear();

            HarmonyPatches.ColorNoteVisualsHandleNoteControllerDidInitEvent.NoteColoursActive = false;
            HarmonyPatches.ObstacleControllerInit.ClearObstacleColors();

            Extensions.SaberColourizer.CurrentAColor = null;
            Extensions.SaberColourizer.CurrentBColor = null;

            ChromaGradientEvent.Clear();

            ClearLightSwitches();
        }
    }
}
