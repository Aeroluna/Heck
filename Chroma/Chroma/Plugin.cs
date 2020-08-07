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
    using static Chroma.NoteColorManager;
    using IPALogger = IPA.Logging.Logger;

    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        internal const string REQUIREMENTNAME = "Chroma";
        internal const string HARMONYID = "com.noodle.BeatSaber.Chroma";

        internal static readonly Harmony HarmonyInstance = new Harmony(HARMONYID);

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
            else
            {
                NoodleExtensionsActive = false;
            }
        }

        [OnDisable]
        public void OnDisable()
        {
            // Harmony patches
            HarmonyInstance.UnpatchAll(HARMONYID);

            GameplaySetup.instance.RemoveTab("Chroma");
            GameplaySetup.instance.RemoveTab("Lightshow Modifiers");

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events", false);
        }

        public void OnActiveSceneChanged(Scene current, Scene next)
        {
            if (next.name == "GameCore")
            {
                ChromaController.Init();
            }
        }

        private static void CleanupSongEvents()
        {
            ChromaGradientEvent.Gradients.Clear();

            HarmonyPatches.ObstacleControllerInit.ClearObstacleColors();

            Extensions.SaberColorizer.CurrentAColor = null;
            Extensions.SaberColorizer.CurrentBColor = null;

            ChromaGradientEvent.Clear();
        }
    }
}
