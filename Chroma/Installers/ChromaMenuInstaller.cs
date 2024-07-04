using Chroma.EnvironmentEnhancement;
using Chroma.EnvironmentEnhancement.Saved;
using Heck;
using JetBrains.Annotations;
using Zenject;

namespace Chroma.Installers
{
    [UsedImplicitly]
    internal class ChromaMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
#if !LATEST
            Container.BindInterfacesTo<EnvironmentMaterialsManager.EnvironmentMaterialsManagerInitializer>().AsSingle(); // what even is this
#endif

            if (HeckController.DebugMode)
            {
                Container.BindInterfacesTo<ReloadListener>().AsSingle();
            }
        }
    }
}
