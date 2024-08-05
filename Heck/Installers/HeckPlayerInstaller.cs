using Heck.Animation;
using Heck.Animation.Events;
using Heck.Animation.Transform;
using Heck.Event;
using Heck.HarmonyPatches;
using Heck.ObjectInitialize;
using JetBrains.Annotations;
using Zenject;

namespace Heck.Installers;

[UsedImplicitly]
internal class HeckPlayerInstaller : Installer
{
    private readonly FeaturesModule _featuresModule;

    private HeckPlayerInstaller(FeaturesModule featuresModule)
    {
        _featuresModule = featuresModule;
    }

    public override void InstallBindings()
    {
        if (!_featuresModule.Active)
        {
            return;
        }

        Container.Bind<ObjectInitializerManager>().AsSingle();

        // Note Cut Sound Fix
        Container.BindInterfacesTo<NoteCutSoundLimiter>().AsSingle();

        // Events
        Container.Bind<CoroutineDummy>().FromNewComponentOnRoot().AsSingle();

        // Custom Events
        Container.BindInterfacesTo<CustomEventController>().AsSingle();
        Container.BindInterfacesTo<CoroutineEvent>().AsSingle();

        // TransformController
        Container.BindInterfacesAndSelfTo<TransformControllerFactory>().AsSingle();

        // Track GameObject Tracker
        Container.BindInterfacesTo<GameObjectTracker>().AsSingle();

        // Track updater
        Container.BindInterfacesTo<TrackUpdateManager>().AsSingle();

        // BurstSliders
        Container.BindInterfacesTo<BurstSliderDataRegisterer>().AsSingle();
    }
}
