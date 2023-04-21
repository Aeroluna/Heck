using Heck.BaseProvider;
using Heck.ReLoad;
using Heck.Settings;
using JetBrains.Annotations;
using Zenject;

namespace Heck.Installers
{
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

            if (!HeckController.DebugMode)
            {
                return;
            }

            Container.BindInstance(_config.ReLoader).AsSingle();
            Container.Bind<ReLoaderLoader>().AsSingle();
            Container.BindInterfacesTo<ReLoadRestart>().AsSingle();
        }
    }
}
