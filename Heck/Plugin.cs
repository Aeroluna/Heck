using System;
using System.Linq;
using System.Reflection;
using CustomJSONData;
using HarmonyLib;
using Heck.Animation;
using Heck.SettingsSetter;
using IPA;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using static Heck.HeckController;

namespace Heck
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        private static readonly Harmony _harmonyInstance = new(HARMONY_ID);

        private static bool _hasInit;
        private static GameScenesManager? _gameScenesManager;

#pragma warning disable CA1822
        [UsedImplicitly]
        [Init]
        public void Init(IPA.Logging.Logger pluginLogger)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            foreach (string arg in arguments)
            {
                if (arg.ToLower() != "-cumdump")
                {
                    continue;
                }

                CumDump = true;
                Log.Logger.Log("[-cumdump] launch argument detected, running in Cum Dump mode.");
            }

            Log.Logger = new HeckLogger(pluginLogger);
            SettingSetterSettableSettingsManager.SetupSettingsTable();
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            CustomDataDeserializer.DeserializeBeatmapData += HeckCustomDataManager.DeserializeCustomEvents;
        }

        [UsedImplicitly]
        [OnEnable]
        public void OnEnable()
        {
            _harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            CustomEventCallbackController.didInitEvent += AnimationController.CustomEventCallbackInit;
        }

        [UsedImplicitly]
        [OnDisable]
        public void OnDisable()
        {
            _harmonyInstance.UnpatchAll(HARMONY_ID);
            CustomEventCallbackController.didInitEvent -= AnimationController.CustomEventCallbackInit;
        }
#pragma warning restore CA1822

        private static void MenuLoadFresh(ScenesTransitionSetupDataSO _1, DiContainer _2)
        {
            SettingsSetterViewController.Instantiate();
            _gameScenesManager!.transitionDidFinishEvent -= MenuLoadFresh;
        }

        private static void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            if (prevScene.name == "PCInit")
            {
                _hasInit = true;
            }

            if (!_hasInit || !nextScene.name.Contains("Menu") || prevScene.name != "EmptyTransition")
            {
                return;
            }

            _hasInit = false;
            if (_gameScenesManager == null)
            {
                _gameScenesManager = Resources.FindObjectsOfTypeAll<GameScenesManager>().FirstOrDefault()
                                     ?? throw new InvalidOperationException("Could not find GameScenesManager.");
            }

            _gameScenesManager.transitionDidFinishEvent += MenuLoadFresh;
        }
    }
}
