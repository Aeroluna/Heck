using System;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using Heck.Animation;
using Heck.Installers;
using Heck.SettingsSetter;
using SiraUtil.Zenject;
using UnityEngine;
using static Heck.HeckController;
using Config = Heck.Settings.Config;

namespace Heck
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("BSIPA_Utilities")]
    [BepInDependency("BeatSaberMarkupLanguage")]
    [BepInDependency("CustomJSONData")]
    [BepInDependency("SiraUtil")]
    [BepInProcess("Beat Saber.exe")]
    internal class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; } = null!;

        private void Awake()
        {
            Log = Logger;
            string[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Any(arg => arg.ToLower() == "-aerolunaisthebestmodder"))
            {
                DebugMode = true;
                Log.LogDebug("[-aerolunaisthebestmodder] launch argument detected, running in Debug mode.");
            }

            SettingSetterSettableSettingsManager.SetupSettingsTable();

            Zenjector zenjector = Zenjector.ConstructZenjector(Info);
            zenjector.Install<HeckAppInstaller>(Location.App, new Config(Config));
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
        private void OnEnable()
        {
            CorePatcher.Enabled = true;
        }

        private void OnDisable()
        {
            CorePatcher.Enabled = false;
            FeaturesPatcher.Enabled = false;
        }
#pragma warning restore CA1822
    }
}
