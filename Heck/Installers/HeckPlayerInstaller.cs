using Heck.Animation;
using Heck.Animation.Events;
using Heck.Animation.Transform;
using JetBrains.Annotations;
using Zenject;

namespace Heck.Installers
{
    [UsedImplicitly]
    internal class HeckPlayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (!HeckController.FeaturesPatcher.Enabled)
            {
                return;
            }

            // Events
            Container.Bind<CoroutineDummy>().FromNewComponentOnRoot().AsSingle();
            Container.Bind<EventController>().AsSingle().NonLazy();
            Container.Bind<CoroutineEventManager>().AsSingle();

            // TransformController
            Container.Bind<TransformControllerFactory>().AsSingle();

            // Track updater
            Container.BindInterfacesTo<TrackUpdateManager>().AsSingle().NonLazy();
        }
    }
}
