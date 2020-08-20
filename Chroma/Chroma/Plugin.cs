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

        internal static bool NoodleExtensionsActive { get; private set; } = false;

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
            // Harmony patches
            _harmonyInstanceCore.PatchAll(Assembly.GetExecutingAssembly());

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

            SceneManager.activeSceneChanged += ChromaController.OnActiveSceneChanged;
        }

        [OnDisable]
        public void OnDisable()
        {
            // Harmony patches
            _harmonyInstanceCore.UnpatchAll(HARMONYID);

            GameplaySetup.instance.RemoveTab("Chroma");
            GameplaySetup.instance.RemoveTab("Lightshow Modifiers");

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events", false);
        }
    }
}
