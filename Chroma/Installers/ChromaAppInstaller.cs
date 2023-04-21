using Chroma.EnvironmentEnhancement;
using Chroma.EnvironmentEnhancement.Saved;
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
            Container.Bind<ChromaSettingsUI>().AsSingle().NonLazy();

            Container.Bind<SavedEnvironmentLoader>().AsSingle();
            Container.Bind<EnvironmentMaterialsManager>().FromNewComponentOnNewGameObject().AsSingle();

            Container.Bind<CustomLevelLoaderExposer>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<ColorBaseProvider>().AsSingle();
        }
    }
}
