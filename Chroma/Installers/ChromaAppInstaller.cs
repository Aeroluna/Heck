using Chroma.EnvironmentEnhancement;
using Chroma.EnvironmentEnhancement.Saved;
using Chroma.HarmonyPatches;
using Chroma.HarmonyPatches.ZenModeWalls;
using Chroma.Modules;
using Chroma.Settings;
using JetBrains.Annotations;
using Zenject;

namespace Chroma.Installers
{
    [UsedImplicitly]
    internal class ChromaAppInstaller : Installer
    {
        private readonly Config _config;

        private ChromaAppInstaller(Config config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config);
            Container.BindInterfacesTo<ChromaSettingsUI>().AsSingle().NonLazy();

            Container.Bind<SavedEnvironmentLoader>().AsSingle();
            Container.Bind<EnvironmentMaterialsManager>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<CustomEnvironmentLoading>().AsSingle();

            Container.BindInterfacesAndSelfTo<ColorBaseProvider>().AsSingle();

            Container.BindInterfacesTo<ForceZenModeObstacleBeatmapData>().AsSingle();

            Container.BindInterfacesAndSelfTo<ColorizerModule>().AsSingle();
            Container.BindInterfacesAndSelfTo<FeaturesModule>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnvironmentModule>().AsSingle();
        }
    }
}
