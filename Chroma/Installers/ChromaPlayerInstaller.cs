using Chroma.Animation;
using Chroma.Colorizer;
using Chroma.EnvironmentEnhancement;
using Chroma.EnvironmentEnhancement.Component;
using Chroma.HarmonyPatches;
using Chroma.HarmonyPatches.Colorizer;
using Chroma.HarmonyPatches.Colorizer.Initialize;
using Chroma.HarmonyPatches.EnvironmentComponent;
using Chroma.HarmonyPatches.Events;
using Chroma.HarmonyPatches.Mirror;
using Chroma.HarmonyPatches.ZenModeWalls;
using Chroma.Lighting;
using Chroma.Modules;
using Heck;
using JetBrains.Annotations;
using Zenject;

namespace Chroma.Installers;

[UsedImplicitly]
internal class ChromaPlayerInstaller : Installer
{
    private readonly ColorizerModule _colorizerModule;
    private readonly EnvironmentModule _environmentModule;
    private readonly FeaturesModule _featuresModule;

    internal ChromaPlayerInstaller(
        ColorizerModule colorizerModule,
        FeaturesModule featuresModule,
        EnvironmentModule environmentModule)
    {
        _colorizerModule = colorizerModule;
        _featuresModule = featuresModule;
        _environmentModule = environmentModule;
    }

    public override void InstallBindings()
    {
        if (_colorizerModule.Active)
        {
            // Colorizer
            Container.Bind<BombColorizerManager>().AsSingle();
            Container.BindFactory<NoteControllerBase, BombColorizer, BombColorizer.Factory>().AsSingle();
            Container.Bind<LightColorizerManager>().AsSingle();
            Container.BindFactory<ChromaLightSwitchEventEffect, LightColorizer, LightColorizer.Factory>().AsSingle();
            Container
                .BindFactory<LightSwitchEventEffect, ChromaLightSwitchEventEffect,
                    ChromaLightSwitchEventEffect.Factory>()
                .FromFactory<DisposableClassFactory<LightSwitchEventEffect, ChromaLightSwitchEventEffect>>();
            Container.BindInterfacesAndSelfTo<NoteColorizerManager>().AsSingle();
            Container.BindFactory<NoteControllerBase, NoteColorizer, NoteColorizer.Factory>().AsSingle();
            Container.Bind<ObstacleColorizerManager>().AsSingle();
            Container.BindFactory<ObstacleControllerBase, ObstacleColorizer, ObstacleColorizer.Factory>().AsSingle();
            Container.Bind<ParticleColorizerManager>().AsSingle();
            Container
                .BindFactory<ParticleSystemEventEffect, ParticleColorizer, ParticleColorizer.Factory>()
                .FromFactory<DisposableClassFactory<ParticleSystemEventEffect, ParticleColorizer>>();
            Container.Bind<SaberColorizerManager>().AsSingle();
            Container.BindFactory<Saber, SaberColorizer, SaberColorizer.Factory>().AsSingle();
            Container.Bind<SaberColorizerIntialize>().AsSingle().NonLazy();
            Container.Bind<SliderColorizerManager>().AsSingle();
            Container.BindFactory<SliderController, SliderColorizer, SliderColorizer.Factory>().AsSingle();
            Container.BindInterfacesTo<ObjectInitializer>().AsSingle();

            // Colorizer Initialize
            Container.BindInterfacesAndSelfTo<LightIDTableManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<LightWithIdRegisterer>().AsSingle();
            Container.BindInterfacesTo<LightColorizerInitialize>().AsSingle();
            Container.BindInterfacesTo<ParticleColorizerInitialize>().AsSingle();

            // Colorizer Patch
            Container.BindInterfacesTo<NoteEffectsColorize>().AsSingle();
            Container.BindInterfacesTo<ObstacleEffectsColorize>().AsSingle();

            // Mirror
            Container.BindInterfacesTo<MirroredNoteChromaTracker>().AsSingle();
            Container.BindInterfacesTo<MirroredObstacleChromaTracker>().AsSingle();

            // Base Provider
            Container.BindInterfacesTo<ColorSchemeGetter>().AsSingle();
        }

        if (_featuresModule.Active)
        {
            // Colorizer Patch
            Container.BindInterfacesTo<ObjectColorize>().AsSingle();
            Container.BindInterfacesTo<NoteObjectColorize>().AsSingle();

            // Events
            Container.BindInterfacesTo<LightPairRotationChromafier>().AsSingle();
            Container.BindInterfacesTo<LightRotationChromafier>().AsSingle();
            Container.BindInterfacesTo<RingRotationChromafier>().AsSingle();
            Container.BindInterfacesTo<RingStepChromafier>().AsSingle();
            Container.BindInterfacesTo<MovementBeatmapChromafier>().AsSingle();
            Container.Bind<ChromaRingsRotationEffect.Factory>().AsSingle();

            // Custom Events
            Container.BindInterfacesTo<AnimateComponent>().AsSingle();
            Container.BindInterfacesTo<FogAnimatorV2>().AsSingle();

            // Disable Spawn Effect
            Container.BindInterfacesTo<BeatEffectSpawnerSkip>().AsSingle();
            Container.BindInterfacesTo<DebrisSpawnerSkip>().AsSingle();

            // Lighting
            Container.BindInterfacesAndSelfTo<ChromaGradientController>().AsSingle();
        }

        if (_environmentModule.Active)
        {
            // EnvironmentComponent
#if PRE_V1_37_1
            Container.BindInterfacesAndSelfTo<BeatmapObjectsAvoidanceTransformOverride>().AsSingle();
#endif
            Container.BindInterfacesAndSelfTo<ParametricBoxControllerTransformOverride>().AsSingle();
            Container.BindInterfacesAndSelfTo<TrackLaneRingOffset>().AsSingle();

            // EnvironmentEnhancement
            ////Container.BindInterfacesTo<CustomEnvironmentLoading>().AsSingle();
            Container.Bind<DuplicateInitializer>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnvironmentEnhancementManager>().AsSingle();
            Container.Bind<ComponentCustomizer>().AsSingle();
            Container.Bind<GeometryFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<MaterialsManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<MaterialColorAnimator>().AsSingle();
            Container.Bind<LightWithIdCustomizer>().AsSingle();
        }

        // Zen mode
        Container.BindInterfacesTo<ObstacleHeadCollisionDisable>().AsSingle();
        Container.BindInterfacesTo<ZenModeBinder>().AsSingle();
    }
}
