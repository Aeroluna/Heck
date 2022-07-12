using Heck.Animation;
using Heck.Animation.Events;
using Heck.Animation.Transform;
using Heck.HarmonyPatches;
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
            Container.BindInterfacesTo<EventController>().AsSingle();
            Container.Bind<CoroutineEventManager>().AsSingle();

            // TransformController
            Container.BindInterfacesAndSelfTo<TransformControllerFactory>().AsSingle();

            // Track updater
            Container.BindInterfacesTo<TrackUpdateManager>().AsSingle().NonLazy();

            // BurstSliders
            Container.BindInterfacesTo<BurstSliderDataRegisterer>().AsSingle();
        }
    }
}
