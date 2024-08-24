using Heck.BaseProvider;
using Heck.Deserialize;
using Heck.HarmonyPatches;
using Heck.HarmonyPatches.ModuleActivator;
using Heck.Module;
using Heck.ReLoad;
using Heck.Settings;
using JetBrains.Annotations;
using Zenject;

namespace Heck.Installers;

[UsedImplicitly]
internal class HeckAppInstaller : Installer
{
    private readonly Config _config;

    private HeckAppInstaller(Config config)
    {
        _config = config;
    }

    public override void InstallBindings()
    {
        Container.Bind<BaseProviderManager>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<ModuleManager>().AsSingle();
#if LATEST
        Container.BindInterfacesTo<StandardModuleActivator>().AsSingle();
        Container.BindInterfacesTo<MissionModuleActivator>().AsSingle();
        Container.BindInterfacesTo<MiscModuleActivator>().AsSingle();
#else
        Container.BindInterfacesTo<SceneTransitionModuleActivator>().AsSingle();
#endif
        Container.Bind<DeserializerManager>().AsSingle();
        Container.BindInterfacesTo<PatchedPlayerInstaller>().AsSingle();

        Container.BindInterfacesAndSelfTo<FeaturesModule>().AsSingle();

#if LATEST
        Container.BindInterfacesAndSelfTo<PerformancePresetOverride>().AsSingle();
#endif

        if (!HeckController.DebugMode)
        {
            return;
        }

        Container.BindInstance(_config.ReLoader).AsSingle();
        Container.Bind<ReLoaderLoader>().AsSingle();
        Container.BindInterfacesTo<ReLoadRestart>().AsSingle();
    }
}
