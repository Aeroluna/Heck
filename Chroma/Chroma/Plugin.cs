namespace Chroma
{
    using System.Reflection;
    using BeatSaberMarkupLanguage.GameplaySetup;
    using Chroma.Settings;
    using Chroma.Utils;
    using HarmonyLib;
    using IPA;
    using IPA.Config;
    using IPA.Config.Stores;
    using UnityEngine.SceneManagement;
    using IPALogger = IPA.Logging.Logger;

    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        internal const string REQUIREMENTNAME = "Chroma";
        internal const string HARMONYIDCORE = "com.noodle.BeatSaber.ChromaCore";
        internal const string HARMONYID = "com.noodle.BeatSaber.Chroma";

        internal static readonly Harmony _harmonyInstanceCore = new Harmony(HARMONYIDCORE);
        internal static readonly Harmony _harmonyInstance = new Harmony(HARMONYID);

        internal static bool NoodleExtensionsInstalled { get; private set; } = false;

        [Init]
        public void Init(IPALogger pluginLogger, Config conf)
        {
            ChromaLogger.IPAlogger = pluginLogger;
            ChromaConfig.Instance = conf.Generated<ChromaConfig>();
            ChromaController.InitChromaPatches();
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmonyInstanceCore.PatchAll(Assembly.GetExecutingAssembly());

            GameplaySetup.instance.AddTab("Chroma", "Chroma.Settings.modifiers.bsml", ChromaSettingsUI.instance);
            if (ChromaConfig.Instance.LightshowMenu)
            {
                GameplaySetup.instance.AddTab("Lightshow Modifiers", "Chroma.Settings.lightshow.bsml", ChromaSettingsUI.instance);
            }

            ChromaUtils.SetSongCoreCapability(REQUIREMENTNAME, ChromaConfig.Instance.CustomColorEventsEnabled);

            SceneManager.activeSceneChanged += ChromaController.OnActiveSceneChanged;

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events");

            if (ChromaUtils.IsModInstalled("NoodleExtensions"))
            {
                AnimationHelper.SubscribeColorEvents();
                NoodleExtensionsInstalled = true;
            }
            else
            {
                NoodleExtensionsInstalled = false;
            }
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmonyInstanceCore.UnpatchAll(HARMONYID);

            GameplaySetup.instance.RemoveTab("Chroma");
            GameplaySetup.instance.RemoveTab("Lightshow Modifiers");

            ChromaUtils.SetSongCoreCapability(REQUIREMENTNAME, false);

            SceneManager.activeSceneChanged -= ChromaController.OnActiveSceneChanged;

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events", false);
        }
    }
}
