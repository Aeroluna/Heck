using Chroma.EnvironmentEnhancement;
using Chroma.EnvironmentEnhancement.Saved;
using Chroma.Settings;
using Heck;
using JetBrains.Annotations;
using Zenject;

namespace Chroma.Installers;

[UsedImplicitly]
internal class ChromaMenuInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<ChromaSettingsUI>().AsSingle();
        Container
            .BindInterfacesTo<EnvironmentMaterialsManager.EnvironmentMaterialsManagerInitializer>()
            .AsSingle(); // what even is this

        if (HeckController.DebugMode)
        {
            Container.BindInterfacesTo<ReloadListener>().AsSingle();
        }
    }
}
