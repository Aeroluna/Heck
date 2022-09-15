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
        public override void InstallBindings()
        {
            Container.Bind<EnvironmentMaterialsManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();

            // Custom Environments
            Container.Bind<Loader>().AsSingle();
            Container.Bind<ChromaSettingsUI>().AsSingle().NonLazy();
            Container.Bind<CustomLevelLoaderExposer>().AsSingle().NonLazy();
        }
    }
}
