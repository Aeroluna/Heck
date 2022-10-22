using Heck.ReLoad;
using JetBrains.Annotations;
using Zenject;

namespace Heck.Installers
{
    [UsedImplicitly]
    internal class HeckAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (!HeckController.DebugMode)
            {
                return;
            }

            Container.Bind<ReLoaderLoader>().AsSingle();
            Container.BindInterfacesTo<ReLoadRestart>().AsSingle();
        }
    }
}
