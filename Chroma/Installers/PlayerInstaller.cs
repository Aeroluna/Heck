using Chroma.Animation;
using Chroma.Colorizer;
using Chroma.HarmonyPatches.Colorizer;
using Chroma.HarmonyPatches.Colorizer.Initialize;
using Chroma.HarmonyPatches.EnvironmentComponent;
using Chroma.HarmonyPatches.Events;
using Chroma.HarmonyPatches.Mirror;
using Chroma.HarmonyPatches.ZenModeWalls;
using Chroma.Lighting;
using Chroma.Lighting.EnvironmentEnhancement;
using Heck.Animation;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Chroma.Installers
{
    [UsedImplicitly]
    internal class PlayerInstaller : Installer
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
                Container.BindFactory<LightSwitchEventEffect, ChromaLightSwitchEventEffect, ChromaLightSwitchEventEffect.Factory>().AsSingle();
                Container.BindInterfacesAndSelfTo<NoteColorizerManager>().AsSingle();
                Container.BindFactory<NoteControllerBase, NoteColorizer, NoteColorizer.Factory>().AsSingle();
                Container.Bind<ObstacleColorizerManager>().AsSingle();
                Container.BindFactory<ObstacleControllerBase, ObstacleColorizer, ObstacleColorizer.Factory>().AsSingle();
                Container.Bind<ParticleColorizerManager>().AsSingle();
                Container.BindFactory<ParticleSystemEventEffect, ParticleColorizer, ParticleColorizer.Factory>().AsSingle();
                Container.Bind<SaberColorizerManager>().AsSingle();
                Container.BindFactory<Saber, SaberColorizer, SaberColorizer.Factory>().AsSingle();
                Container.Bind<SaberColorizerIntialize>().AsSingle().NonLazy();
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
                Container.Bind<EventController>().AsSingle().NonLazy();
                Container.BindInterfacesAndSelfTo<ChromaFogController>().AsSingle();

                // Colorizer Patch
                Container.BindInterfacesTo<ObjectColorize>().AsSingle();

                // EnvironmentComponent
                Container.BindInterfacesTo<BeatmapObjectsAvoidanceTransformOverride>().AsSingle();
                Container.BindInterfacesTo<ParametricBoxControllerTransformOverride>().AsSingle();
                Container.BindInterfacesTo<TrackLaneRingOffset>().AsSingle();
                Container.BindInterfacesAndSelfTo<TrackLaneRingsManagerTracker>().AsSingle();

                // Events
                Container.BindInterfacesTo<LightPairRotationChromafier>().AsSingle();
                Container.BindInterfacesTo<LightRotationChromafier>().AsSingle();
                Container.BindInterfacesTo<RingRotationChromafier>().AsSingle();
                Container.BindFactory<TrackLaneRingsRotationEffect, ChromaRingsRotationEffect, ChromaRingsRotationEffect.Factory>()
                    .FromFactory<ChromaRingsRotationEffect.ChromaRingFactory>();
                Container.BindInterfacesTo<RingStepChromafier>().AsSingle();

                // Disable Spawn Effect
                Container.BindInterfacesTo<BeatEffectSpawner>().AsSingle();

                // Lighting
                Container.BindInterfacesAndSelfTo<ChromaGradientController>().AsSingle();
                Container.BindFactory<
                    Color,
                    Color,
                    float,
                    float,
                    BasicBeatmapEventType,
                    Functions,
                    ChromaGradientController.ChromaGradientEvent,
                    ChromaGradientController.ChromaGradientEvent.Factory>().AsSingle();

                // EnvironmentEnhancement
                Container.Bind<ComponentInitializer>().AsSingle();
                Container.BindInterfacesAndSelfTo<EnvironmentEnhancementManager>().AsSingle().NonLazy();
                Container.BindFactory<GameObject, GameObjectTrackController, GameObjectTrackController.Factory>()
                    .FromFactory<GameObjectTrackController.GameObjectTrackControllerFactory>();
                Container.Bind<ParametricBoxControllerParameters>().AsSingle();
            }

            // Zen mode
            Container.BindInterfacesTo<ObstacleHeadCollisionDisable>().AsSingle();
        }
    }
}
