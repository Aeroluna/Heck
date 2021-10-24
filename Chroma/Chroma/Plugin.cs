namespace Chroma
{
    using System.Linq;
    using System.Reflection;
    using BeatSaberMarkupLanguage.GameplaySetup;
    using Chroma.Settings;
    using Chroma.Utils;
    using HarmonyLib;
    using Heck;
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
        internal const string LERPTYPE = "_lerpType";
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

        internal const string ENVIRONMENT = "_environment";
        internal const string ID = "_id";
        internal const string LOOKUPMETHOD = "_lookupMethod";
        internal const string DUPLICATIONAMOUNT = "_duplicate";
        internal const string ACTIVE = "_active";
        internal const string SCALE = "_scale";
        internal const string POSITION = "_position";
        internal const string LOCALPOSITION = "_localPosition";
        internal const string OBJECTROTATION = "_rotation";
        internal const string LOCALROTATION = "_localRotation";

        internal static readonly Harmony _harmonyInstanceCore = new Harmony(HARMONYIDCORE);
        internal static readonly Harmony _harmonyInstance = new Harmony(HARMONYID);

        internal static bool SiraUtilInstalled { get; private set; } = false;

#pragma warning disable CS8618
        internal static HeckLogger Logger { get; private set; }
#pragma warning restore CS8618

        [Init]
        public void Init(IPALogger pluginLogger, Config conf)
        {
            Logger = new HeckLogger(pluginLogger);
            ChromaSettableSettings.SetupSettableSettings();
            ChromaConfig.Instance = conf.Generated<ChromaConfig>();
            HeckData.InitPatches(_harmonyInstance, Assembly.GetExecutingAssembly());
            LightIDTableManager.InitTable();
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmonyInstanceCore.PatchAll(Assembly.GetExecutingAssembly());

            SiraUtilInstalled = IPA.Loader.PluginManager.EnabledPlugins.Any(x => x.Id == "SiraUtil");

            GameplaySetup.instance.AddTab("Chroma", "Chroma.Settings.modifiers.bsml", ChromaSettingsUI.instance);

            SceneManager.activeSceneChanged += ChromaController.OnActiveSceneChanged;

            Heck.Animation.TrackBuilder.TrackCreated += AnimationHelper.OnTrackCreated;
            Heck.Animation.TrackBuilder.TrackManagerCreated += EnvironmentEnhancementManager.CreateEnvironmentTracks;
            CustomDataDeserializer.OnDeserializeBeatmapObjectDatas += ChromaCustomDataManager.DeserializeBeatmapObjects;
            CustomDataDeserializer.OnDeserializeBeatmapEventDatas += ChromaCustomDataManager.DeserializeBeatmapEvents;

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events");
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmonyInstanceCore.UnpatchAll(HARMONYID);

            GameplaySetup.instance.RemoveTab("Chroma");

            ChromaUtils.SetSongCoreCapability(REQUIREMENTNAME, false);

            SceneManager.activeSceneChanged -= ChromaController.OnActiveSceneChanged;

            Heck.Animation.TrackBuilder.TrackCreated -= AnimationHelper.OnTrackCreated;
            Heck.Animation.TrackBuilder.TrackManagerCreated -= EnvironmentEnhancementManager.CreateEnvironmentTracks;
            CustomDataDeserializer.OnDeserializeBeatmapObjectDatas -= ChromaCustomDataManager.DeserializeBeatmapObjects;
            CustomDataDeserializer.OnDeserializeBeatmapEventDatas -= ChromaCustomDataManager.DeserializeBeatmapEvents;

            // Legacy support
            ChromaUtils.SetSongCoreCapability("Chroma Lighting Events", false);
        }
    }
}
