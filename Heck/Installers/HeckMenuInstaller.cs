using Heck.HarmonyPatches;
using Heck.ReLoad;
using Heck.SettingsSetter;
using JetBrains.Annotations;
using Zenject;

namespace Heck.Installers
{
    [UsedImplicitly]
    internal class HeckMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (HeckController.DebugMode)
            {
                Container.BindInterfacesTo<MenuReLoad>().AsSingle();
            }

            // Settings Setter
            Container.Bind<SettingsSetterViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesTo<SettableSettingsUI>().AsSingle();
        }
    }
}
