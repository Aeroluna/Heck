using Heck.HarmonyPatches;
using Heck.PlayView;
using Heck.ReLoad;
using Heck.SettingsSetter;
using Heck.VersionCheck;
using JetBrains.Annotations;
using Zenject;

namespace Heck.Installers;

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
        Container.BindInterfacesAndSelfTo<PlayViewManager>().AsSingle();
        Container.BindInterfacesTo<SettingsSetterViewController>().FromNewComponentAsViewController().AsSingle();
        Container.BindInterfacesTo<PlayViewInterrupter>().AsSingle();
        Container.BindInterfacesAndSelfTo<FlowCoordinatorTransitionListener>().AsSingle();

        // Mod Version Check
        Container.BindInterfacesTo<VersionCheckViewController>().FromNewComponentAsViewController().AsSingle();

#if !PRE_V1_37_1
        if (!HeckController.DebugMode)
        {
            return;
        }

        Container.BindInterfacesAndSelfTo<SongCoreCacheReLoader>().AsSingle();
#endif
    }
}
