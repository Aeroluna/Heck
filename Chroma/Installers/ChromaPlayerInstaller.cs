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
using Heck;
using JetBrains.Annotations;
using Zenject;

namespace Chroma.Installers
{
    [UsedImplicitly]
    internal class ChromaPlayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (ChromaController.ColorizerPatcher.Enabled)
            {
                // Colorizer
                Container.Bind<BombColorizerManager>().AsSingle();
                Container.BindFactory<NoteControllerBase, BombColorizer, BombColorizer.Factory>().AsSingle();
                Container.Bind<LightColorizerManager>().AsSingle();
                Container.BindFactory<ChromaLightSwitchEventEffect, LightColorizer, LightColorizer.Factory>().AsSingle();
                Container.BindFactory<LightSwitchEventEffect, ChromaLightSwitchEventEffect, ChromaLightSwitchEventEffect.Factory>()
                    .FromFactory<DisposableClassFactory<LightSwitchEventEffect, ChromaLightSwitchEventEffect>>();
                Container.BindInterfacesAndSelfTo<NoteColorizerManager>().AsSingle();
                Container.BindFactory<NoteControllerBase, NoteColorizer, NoteColorizer.Factory>().AsSingle();
                Container.Bind<ObstacleColorizerManager>().AsSingle();
                Container.BindFactory<ObstacleControllerBase, ObstacleColorizer, ObstacleColorizer.Factory>().AsSingle();
                Container.Bind<ParticleColorizerManager>().AsSingle();
                Container.BindFactory<ParticleSystemEventEffect, ParticleColorizer, ParticleColorizer.Factory>()
                    .FromFactory<DisposableClassFactory<ParticleSystemEventEffect, ParticleColorizer>>();
                Container.Bind<SaberColorizerManager>().AsSingle();
                Container.BindFactory<Saber, SaberColorizer, SaberColorizer.Factory>().AsSingle();
                Container.Bind<SaberColorizerIntialize>().AsSingle().NonLazy();
                Container.Bind<SliderColorizerManager>().AsSingle();
                Container.BindFactory<SliderController, SliderColorizer, SliderColorizer.Factory>().AsSingle();
                Container.BindInterfacesTo<ObjectInitializer>().AsSingle();

                // Colorizer Initialize
                Container.BindInterfacesAndSelfTo<LightWithIdRegisterer>().AsSingle();
                Container.BindInterfacesTo<LightColorizerInitialize>().AsSingle();
                Container.BindInterfacesTo<ParticleColorizerInitialize>().AsSingle();

                // Colorizer Patch
                Container.BindInterfacesTo<NoteEffectsColorize>().AsSingle();
                Container.BindInterfacesTo<ObstacleEffectsColorize>().AsSingle();

                // Mirror
                Container.BindInterfacesTo<MirroredNoteChromaTracker>().AsSingle();
                Container.BindInterfacesTo<MirroredObstacleChromaTracker>().AsSingle();
            }

            if (ChromaController.FeaturesPatcher.Enabled)
            {
                // Animation
                Container.BindInterfacesTo<EventController>().AsSingle();
                Container.BindInterfacesAndSelfTo<FogAnimatorV2>().AsSingle();
                Container.Bind<AnimateComponentEvent>().AsSingle();

                // Colorizer Patch
                Container.BindInterfacesTo<ObjectColorize>().AsSingle();
                Container.BindInterfacesTo<NoteObjectColorize>().AsSingle();

                // EnvironmentComponent
                Container.BindInterfacesAndSelfTo<BeatmapObjectsAvoidanceTransformOverride>().AsSingle();
                Container.BindInterfacesAndSelfTo<ParametricBoxControllerTransformOverride>().AsSingle();
                Container.BindInterfacesAndSelfTo<TrackLaneRingOffset>().AsSingle();
                Container.BindInterfacesAndSelfTo<TrackLaneRingsManagerTracker>().AsSingle();

                // Events
                Container.BindInterfacesTo<LightPairRotationChromafier>().AsSingle();
                Container.BindInterfacesTo<LightRotationChromafier>().AsSingle();
                Container.BindInterfacesTo<RingRotationChromafier>().AsSingle();
                Container.BindInterfacesTo<RingStepChromafier>().AsSingle();
                Container.Bind<ChromaRingsRotationEffect.Factory>().AsSingle();

                // Disable Spawn Effect
                Container.BindInterfacesTo<BeatEffectSpawnerSkip>().AsSingle();

                // Lighting
                Container.BindInterfacesAndSelfTo<ChromaGradientController>().AsSingle();

                // EnvironmentEnhancement
                Container.Bind<DuplicateInitializer>().AsSingle();
                Container.BindInterfacesAndSelfTo<EnvironmentEnhancementManager>().AsSingle().NonLazy();
                Container.Bind<ComponentCustomizer>().AsSingle();
                Container.Bind<GeometryFactory>().AsSingle();
                Container.Bind<MaterialsManager>().AsSingle();
                Container.BindInterfacesAndSelfTo<MaterialColorAnimator>().AsSingle();
                Container.Bind<ILightWithIdCustomizer>().AsSingle();
            }

            // Zen mode
            Container.BindInterfacesTo<ObstacleHeadCollisionDisable>().AsSingle();
        }
    }
}
