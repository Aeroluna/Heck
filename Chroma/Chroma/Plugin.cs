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

        internal static bool NoodleExtensionsActive { get; private set; } = false;

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

            ChromaUtils.SetSongCoreCapability("Chroma");

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events");

            if (ChromaUtils.IsModInstalled("NoodleExtensions"))
            {
                AnimationHelper.SubscribeColorEvents();
                NoodleExtensionsActive = true;
            }
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
            RemoveNoteTypeColorOverride(NoteType.NoteA);
            RemoveNoteTypeColorOverride(NoteType.NoteB);
        }

        private static void OnSongLoaded()
        {
            RemoveNoteTypeColorOverride(NoteType.NoteA);
            RemoveNoteTypeColorOverride(NoteType.NoteB);
        }

        private static void CleanupSongEvents()
        {
            ChromaNoteColorEvent.SavedNoteColors.Clear();
            ChromaGradientEvent.Gradients.Clear();

            HarmonyPatches.ColorNoteVisualsHandleNoteControllerDidInitEvent.NoteColorsActive = false;
            HarmonyPatches.ObstacleControllerInit.ClearObstacleColors();

            Extensions.SaberColorizer.CurrentAColor = null;
            Extensions.SaberColorizer.CurrentBColor = null;

            ChromaGradientEvent.Clear();

            ClearLightSwitches();
        }
    }
}
