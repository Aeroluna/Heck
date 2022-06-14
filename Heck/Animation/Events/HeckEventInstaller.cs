using JetBrains.Annotations;
using Zenject;

namespace Heck.Animation.Events
{
    [UsedImplicitly]
    internal class HeckEventInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (!HeckController.FeaturesPatcher.Enabled)
            {
                return;
            }

            Container.Bind<CoroutineDummy>().FromNewComponentOnRoot().AsSingle();
            Container.Bind<EventController>().AsSingle().NonLazy();
            Container.Bind<CoroutineEventManager>().AsSingle();
        }
    }
}
