namespace Heck
{
    using System.Linq;
    using System.Reflection;
    using HarmonyLib;
    using IPA;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Zenject;
    using IPALogger = IPA.Logging.Logger;

    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        internal const string HARMONYID = "com.aeroluna.BeatSaber.Heck";

        internal const string DURATION = "_duration";
        internal const string EASING = "_easing";
        internal const string NAME = "_name";
        internal const string POINTDEFINITIONS = "_pointDefinitions";
        internal const string POINTS = "_points";
        internal const string TIME = "_time";
        internal const string TRACK = "_track";

        internal const string ANIMATETRACK = "AnimateTrack";
        internal const string ASSIGNPATHANIMATION = "AssignPathAnimation";

        internal static readonly Harmony _harmonyInstance = new Harmony(HARMONYID);

        private static bool _hasInited;
        private static GameScenesManager? _gameScenesManager;

#pragma warning disable CS8618
        internal static HeckLogger Logger { get; private set; }
#pragma warning restore CS8618

        [Init]
        public void Init(IPALogger pluginLogger)
        {
            Logger = new HeckLogger(pluginLogger);
            SettingsSetter.SettingSetterSettableSettingsManager.SetupSettingsTable();
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            CustomJSONData.CustomEventCallbackController.didInitEvent += Animation.AnimationController.CustomEventCallbackInit;
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmonyInstance.UnpatchAll(HARMONYID);
            CustomJSONData.CustomEventCallbackController.didInitEvent -= Animation.AnimationController.CustomEventCallbackInit;
        }

        public void MenuLoadFresh(ScenesTransitionSetupDataSO _1, DiContainer _2)
        {
            SettingsSetter.SettingsSetterViewController.Instantiate();
            _gameScenesManager!.transitionDidFinishEvent -= MenuLoadFresh;
        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            if (prevScene.name == "PCInit")
            {
                _hasInited = true;
            }

            if (_hasInited && nextScene.name.Contains("Menu") && prevScene.name == "EmptyTransition")
            {
                _hasInited = false;
                if (_gameScenesManager == null)
                {
                    _gameScenesManager = Resources.FindObjectsOfTypeAll<GameScenesManager>().FirstOrDefault();
                }

                _gameScenesManager.transitionDidFinishEvent += MenuLoadFresh;
            }
        }
    }
}
