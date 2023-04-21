using JetBrains.Annotations;
using Zenject;

namespace NoodleExtensions.Installers
{
    [UsedImplicitly]
    internal class NoodleAppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<NoodleBaseProvider>().AsSingle();
        }
    }
}
