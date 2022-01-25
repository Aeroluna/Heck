using Heck.HarmonyPatches;
using JetBrains.Annotations;
using Zenject;

namespace Heck.SettingsSetter
{
    [UsedImplicitly]
    internal class SettingsSetterInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<SettingsSetterViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesTo<SettableSettingsUI>().AsSingle();
        }
    }
}
