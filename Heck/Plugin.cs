using System;
using System.Linq;
using Heck.Animation;
using Heck.Installers;
using Heck.Settings;
using Heck.SettingsSetter;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using JetBrains.Annotations;
using SiraUtil.Zenject;
using UnityEngine;
using static Heck.HeckController;
using Logger = IPA.Logging.Logger;

namespace Heck
{
    // unsure why this stuff cant be static or private
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        [UsedImplicitly]
        [Init]
        public Plugin(Logger pluginLogger, Config conf, Zenjector zenjector)
        {
            Log.Logger = new HeckLogger(pluginLogger);

            string[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Any(arg => arg.ToLower() == "-aerolunaisthebestmodder"))
            {
                DebugMode = true;
                Log.Logger.Log("[-aerolunaisthebestmodder] launch argument detected, running in Debug mode.");
                HeckConfig.Instance = conf.Generated<HeckConfig>();
            }

            SettingSetterSettableSettingsManager.SetupSettingsTable();

            zenjector.Install<HeckAppInstaller>(Location.App);
            zenjector.Install<HeckPlayerInstaller>(Location.Player);
            zenjector.Install<HeckMenuInstaller>(Location.Menu);
            zenjector.Expose<NoteCutSoundEffectManager>("Gameplay");

            ModuleManager.Register<ModuleCallbacks>("Heck", 0, RequirementType.None);

            Track.RegisterProperty<Vector3>(POSITION, V2_POSITION);
            Track.RegisterProperty<Vector3>(LOCAL_POSITION, V2_LOCAL_POSITION);
            Track.RegisterProperty<Quaternion>(ROTATION, V2_ROTATION);
            Track.RegisterProperty<Quaternion>(LOCAL_ROTATION, V2_LOCAL_ROTATION);
            Track.RegisterProperty<Vector3>(SCALE, V2_SCALE);
        }

#pragma warning disable CA1822
        [UsedImplicitly]
        [OnEnable]
        public void OnEnable()
        {
            CorePatcher.Enabled = true;
        }

        [UsedImplicitly]
        [OnDisable]
        public void OnDisable()
        {
            CorePatcher.Enabled = false;
            FeaturesPatcher.Enabled = false;
        }
#pragma warning restore CA1822
    }
}
