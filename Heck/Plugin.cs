using System;
using System.Linq;
using Heck.Installers;
using Heck.SettingsSetter;
using IPA;
using IPA.Logging;
using JetBrains.Annotations;
using SiraUtil.Zenject;
using static Heck.HeckController;

namespace Heck
{
    // unsure why this stuff cant be static or private
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        [UsedImplicitly]
        [Init]
        public Plugin(Logger pluginLogger, Zenjector zenjector)
        {
            Log.Logger = new HeckLogger(pluginLogger);

            string[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Any(arg => arg.ToLower() == "-aerolunaisthebestmodder"))
            {
                DebugMode = true;
                Log.Logger.Log("[-aerolunaisthebestmodder] launch argument detected, running in Debug mode.");
            }

            SettingSetterSettableSettingsManager.SetupSettingsTable();

            zenjector.Install<HeckPlayerInstaller>(Location.Player);
            zenjector.Install<HeckSettingsSetterInstaller>(Location.Menu);

            ModuleManager.Register<ModuleCallbacks>("Heck", 0, RequirementType.None);
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
