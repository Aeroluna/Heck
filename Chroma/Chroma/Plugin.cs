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

        internal const string ANIMATION = "_animation";
        internal const string COLOR = "_color";
        internal const string COUNTERSPIN = "_counterSpin";
        internal const string DIRECTION = "_direction";
        internal const string DISABLESPAWNEFFECT = "_disableSpawnEffect";
        internal const string DURATION = "_duration";
        internal const string EASING = "_easing";
        internal const string ENDCOLOR = "_endColor";
        internal const string ENVIRONMENTREMOVAL = "_environmentRemoval";
        internal const string LIGHTGRADIENT = "_lightGradient";
        internal const string LIGHTID = "_lightID";
        internal const string LOCKPOSITION = "_lockPosition";
        internal const string NAMEFILTER = "_nameFilter";
        internal const string PRECISESPEED = "_preciseSpeed";
        internal const string PROP = "_prop";
        internal const string PROPAGATIONID = "_propID";
        internal const string PROPMULT = "_propMult";
        internal const string RESET = "_reset";
        internal const string SPEED = "_speed";
        internal const string SPEEDMULT = "_speedMult";
        internal const string STARTCOLOR = "_startColor";
        internal const string STEP = "_step";
        internal const string STEPMULT = "_stepMult";
        internal const string ROTATION = "_rotation";

        internal static readonly Harmony _harmonyInstanceCore = new Harmony(HARMONYIDCORE);
        internal static readonly Harmony _harmonyInstance = new Harmony(HARMONYID);

        internal static bool NoodleExtensionsInstalled { get; private set; } = false;

        [Init]
        public void Init(IPALogger pluginLogger, Config conf)
        {
            ChromaLogger.IPAlogger = pluginLogger;
            ChromaConfig.Instance = conf.Generated<ChromaConfig>();
            ChromaController.InitChromaPatches();
            LightIDTableManager.InitTable();
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmonyInstanceCore.PatchAll(Assembly.GetExecutingAssembly());

            GameplaySetup.instance.AddTab("Chroma", "Chroma.Settings.modifiers.bsml", ChromaSettingsUI.instance);

            ChromaUtils.SetSongCoreCapability(REQUIREMENTNAME, ChromaConfig.Instance.CustomColorEventsEnabled);

            SceneManager.activeSceneChanged += ChromaController.OnActiveSceneChanged;

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events");

            if (ChromaUtils.IsNoodleExtensionsInstalled())
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
