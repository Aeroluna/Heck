namespace Chroma
{
    using System.Collections.Generic;
    using System.Linq;
    using Chroma.Colorizer;
    using IPA.Utilities;
    using UnityEngine;

    internal static class ComponentInitializer
    {
        private static readonly FieldAccessor<LightWithIdMonoBehaviour, LightWithIdManager>.Accessor _lightWithIdMonoBehaviourManagerAccessor = FieldAccessor<LightWithIdMonoBehaviour, LightWithIdManager>.GetAccessor("_lightManager");
        private static readonly FieldAccessor<LightWithIds, LightWithIdManager>.Accessor _lightWithIdsManagerAccessor = FieldAccessor<LightWithIds, LightWithIdManager>.GetAccessor("_lightManager");
        private static readonly FieldAccessor<TrackLaneRingsManager, TrackLaneRing[]>.Accessor _ringsAccessor = FieldAccessor<TrackLaneRingsManager, TrackLaneRing[]>.GetAccessor("_rings");
        private static readonly FieldAccessor<Spectrogram, BasicSpectrogramData>.Accessor _spectrogramDataAccessor = FieldAccessor<Spectrogram, BasicSpectrogramData>.GetAccessor("_spectrogramData");
        private static readonly FieldAccessor<LightRotationEventEffect, IBeatmapObjectCallbackController>.Accessor _lightCallbackControllerAccessor = FieldAccessor<LightRotationEventEffect, IBeatmapObjectCallbackController>.GetAccessor("_beatmapObjectCallbackController");
        private static readonly FieldAccessor<LightPairRotationEventEffect, IBeatmapObjectCallbackController>.Accessor _lightPairCallbackControllerAccessor = FieldAccessor<LightPairRotationEventEffect, IBeatmapObjectCallbackController>.GetAccessor("_beatmapObjectCallbackController");
        private static readonly FieldAccessor<LightPairRotationEventEffect, Transform>.Accessor _transformLAccessor = FieldAccessor<LightPairRotationEventEffect, Transform>.GetAccessor("_transformL");
        private static readonly FieldAccessor<LightPairRotationEventEffect, Transform>.Accessor _transformRAccessor = FieldAccessor<LightPairRotationEventEffect, Transform>.GetAccessor("_transformR");
        private static readonly FieldAccessor<ParticleSystemEventEffect, IBeatmapObjectCallbackController>.Accessor _particleCallbackControllerAccessor = FieldAccessor<ParticleSystemEventEffect, IBeatmapObjectCallbackController>.GetAccessor("_beatmapObjectCallbackController");
        private static readonly FieldAccessor<ParticleSystemEventEffect, ParticleSystem>.Accessor _particleSystemAccessor = FieldAccessor<ParticleSystemEventEffect, ParticleSystem>.GetAccessor("_particleSystem");
        private static readonly FieldAccessor<Mirror, MirrorRendererSO>.Accessor _mirrorRendererAccessor = FieldAccessor<Mirror, MirrorRendererSO>.GetAccessor("_mirrorRenderer");
        private static readonly FieldAccessor<Mirror, Material>.Accessor _mirrorMaterialAccessor = FieldAccessor<Mirror, Material>.GetAccessor("_mirrorMaterial");
        private static readonly FieldAccessor<TrackLaneRingsPositionStepEffectSpawner, TrackLaneRingsManager>.Accessor _stepSpawnerRingsManagerAccessor = FieldAccessor<TrackLaneRingsPositionStepEffectSpawner, TrackLaneRingsManager>.GetAccessor("_trackLaneRingsManager");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffectSpawner, TrackLaneRingsRotationEffect>.Accessor _trackLaneRingsRotationEffectAccessor = FieldAccessor<TrackLaneRingsRotationEffectSpawner, TrackLaneRingsRotationEffect>.GetAccessor("_trackLaneRingsRotationEffect");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffectSpawner, IBeatmapObjectCallbackController>.Accessor _rotationEffectSpawnerCallbackControllerAccessor = FieldAccessor<TrackLaneRingsRotationEffectSpawner, IBeatmapObjectCallbackController>.GetAccessor("_beatmapObjectCallbackController");

        private static readonly FieldAccessor<TrackLaneRing, Transform>.Accessor _ringTransformAccessor = FieldAccessor<TrackLaneRing, Transform>.GetAccessor("_transform");
        private static readonly FieldAccessor<TrackLaneRing, Vector3>.Accessor _positionOffsetAccessor = FieldAccessor<TrackLaneRing, Vector3>.GetAccessor("_positionOffset");
        private static readonly FieldAccessor<TrackLaneRing, float>.Accessor _posZAccessor = FieldAccessor<TrackLaneRing, float>.GetAccessor("_posZ");

        internal static bool SkipAwake { get; private set; }

        internal static void PrefillComponentsData(Transform root, List<IComponentData> componentDatas)
        {
            SkipAwake = true;

            TrackLaneRingsManager trackLaneRingsManager = root.GetComponent<TrackLaneRingsManager>();
            if (trackLaneRingsManager != null)
            {
                componentDatas.Add(new TrackLaneRingsManagerComponentData()
                {
                    OldTrackLaneRingsManager = trackLaneRingsManager,
                });
            }

            foreach (Transform transform in root)
            {
                PrefillComponentsData(transform, componentDatas);
            }
        }

        internal static void PostfillComponentsData(Transform root, Transform original, List<IComponentData> componentDatas)
        {
            SkipAwake = false;

            TrackLaneRingsManager trackLaneRingsManager = root.GetComponent<TrackLaneRingsManager>();
            if (trackLaneRingsManager != null)
            {
                TrackLaneRingsManager originalManager = original.GetComponent<TrackLaneRingsManager>();
                foreach (TrackLaneRingsManagerComponentData componentData in componentDatas.OfType<TrackLaneRingsManagerComponentData>().Where(n => n.OldTrackLaneRingsManager == originalManager))
                {
                    componentData.NewTrackLaneRingsManager = trackLaneRingsManager;
                }
            }

            TrackLaneRingsRotationEffect rotationEffect = root.GetComponent<TrackLaneRingsRotationEffect>();
            if (rotationEffect != null)
            {
                Object.Destroy(rotationEffect);
            }

            foreach (Transform transform in root)
            {
                int index = transform.GetSiblingIndex();
                PostfillComponentsData(transform, original.GetChild(index), componentDatas);
            }
        }

        internal static void InitializeComponents(Transform root, Transform original, List<GameObjectInfo> gameObjectInfos, List<IComponentData> componentDatas)
        {
            LightWithIdMonoBehaviour lightWithIdMonoBehaviour = root.GetComponent<LightWithIdMonoBehaviour>();
            if (lightWithIdMonoBehaviour != null)
            {
                LightWithIdMonoBehaviour originalLight = original.GetComponent<LightWithIdMonoBehaviour>();
                _lightWithIdMonoBehaviourManagerAccessor(ref lightWithIdMonoBehaviour) = _lightWithIdMonoBehaviourManagerAccessor(ref originalLight);
                LightColorizer.RegisterLight(lightWithIdMonoBehaviour);
            }

            LightWithIds lightWithIds = root.GetComponent<LightWithIds>();
            if (lightWithIds != null)
            {
                LightWithIds originalLight = original.GetComponent<LightWithIds>();
                _lightWithIdsManagerAccessor(ref lightWithIds) = _lightWithIdsManagerAccessor(ref originalLight);
                LightColorizer.RegisterLight(lightWithIds);
            }

            TrackLaneRing trackLaneRing = root.GetComponent<TrackLaneRing>();
            if (trackLaneRing != null)
            {
                TrackLaneRing originalRing = original.GetComponent<TrackLaneRing>();

                if (EnvironmentEnhancementManager.RingRotationOffsets.TryGetValue(originalRing, out Vector3 offset))
                {
                    EnvironmentEnhancementManager.RingRotationOffsets.Add(trackLaneRing, offset);
                }

                _ringTransformAccessor(ref trackLaneRing) = root;
                _positionOffsetAccessor(ref trackLaneRing) = _positionOffsetAccessor(ref originalRing);
                _posZAccessor(ref trackLaneRing) = _posZAccessor(ref originalRing);

                TrackLaneRingsManager managerToAdd = null;
                foreach (TrackLaneRingsManager manager in HarmonyPatches.TrackLaneRingsManagerAwake.RingManagers)
                {
                    TrackLaneRingsManagerComponentData componentData = componentDatas.OfType<TrackLaneRingsManagerComponentData>().Where(n => n.OldTrackLaneRingsManager == manager).FirstOrDefault();
                    if (componentData != null)
                    {
                        managerToAdd = componentData.NewTrackLaneRingsManager;
                    }
                    else
                    {
                        TrackLaneRingsManager managerRef = manager;
                        TrackLaneRing[] rings = _ringsAccessor(ref managerRef);
                        if (rings.Contains(originalRing))
                        {
                            managerToAdd = manager;
                        }
                    }

                    if (managerToAdd != null)
                    {
                        // ToList() to add and then back ToArray()
                        TrackLaneRing[] rings = _ringsAccessor(ref managerToAdd);
                        List<TrackLaneRing> ringsList = rings?.ToList() ?? new List<TrackLaneRing>();
                        ringsList.Add(trackLaneRing);
                        _ringsAccessor(ref managerToAdd) = ringsList.ToArray();

                        if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                        {
                            ChromaLogger.Log($"Initialized TrackLaneRing");
                        }

                        break;
                    }
                }
            }

            TrackLaneRingsPositionStepEffectSpawner positionStepEffectSpawner = root.GetComponent<TrackLaneRingsPositionStepEffectSpawner>();
            if (positionStepEffectSpawner != null)
            {
                foreach (TrackLaneRingsManager manager in HarmonyPatches.TrackLaneRingsManagerAwake.RingManagers)
                {
                    TrackLaneRingsManagerComponentData componentData = componentDatas.OfType<TrackLaneRingsManagerComponentData>().Where(n => n.OldTrackLaneRingsManager == manager).FirstOrDefault();
                    if (componentData != null)
                    {
                        _stepSpawnerRingsManagerAccessor(ref positionStepEffectSpawner) = componentData.NewTrackLaneRingsManager;

                        if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                        {
                            ChromaLogger.Log($"Initialized TrackLaneRingsPositionStepEffectSpawner");
                        }

                        break;
                    }
                }
            }

            ChromaRingsRotationEffect ringsRotationEffect = root.GetComponent<ChromaRingsRotationEffect>();
            if (ringsRotationEffect != null)
            {
                foreach (TrackLaneRingsManager manager in HarmonyPatches.TrackLaneRingsManagerAwake.RingManagers)
                {
                    TrackLaneRingsManagerComponentData componentData = componentDatas.OfType<TrackLaneRingsManagerComponentData>().Where(n => n.OldTrackLaneRingsManager == manager).FirstOrDefault();
                    if (componentData != null)
                    {
                        ringsRotationEffect.SetNewRingManager(componentData.NewTrackLaneRingsManager);

                        if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                        {
                            ChromaLogger.Log($"Initialized ChromaRingsRotationEffect");
                        }

                        break;
                    }
                }

                TrackLaneRingsRotationEffectSpawner rotationEffectSpawner = root.GetComponent<TrackLaneRingsRotationEffectSpawner>();
                if (rotationEffectSpawner != null)
                {
                    TrackLaneRingsRotationEffectSpawner originalRotationEffectSpawner = original.GetComponent<TrackLaneRingsRotationEffectSpawner>();

                    _rotationEffectSpawnerCallbackControllerAccessor(ref rotationEffectSpawner) = _rotationEffectSpawnerCallbackControllerAccessor(ref originalRotationEffectSpawner);
                    _trackLaneRingsRotationEffectAccessor(ref rotationEffectSpawner) = ringsRotationEffect;

                    if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                    {
                        ChromaLogger.Log($"Initialized TrackLaneRingsRotationEffectSpawner");
                    }
                }
            }

            Spectrogram spectrogram = root.GetComponent<Spectrogram>();
            if (spectrogram != null)
            {
                Spectrogram originalSpectrogram = original.GetComponent<Spectrogram>();

                _spectrogramDataAccessor(ref spectrogram) = _spectrogramDataAccessor(ref originalSpectrogram);

                if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                {
                    ChromaLogger.Log($"Initialized Spectrogram");
                }
            }

            LightRotationEventEffect lightRotationEvent = root.GetComponent<LightRotationEventEffect>();
            if (lightRotationEvent != null)
            {
                LightRotationEventEffect originalLightRotationEvent = original.GetComponent<LightRotationEventEffect>();

                _lightCallbackControllerAccessor(ref lightRotationEvent) = _lightCallbackControllerAccessor(ref originalLightRotationEvent);

                if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                {
                    ChromaLogger.Log($"Initialized LightRotationEventEffect");
                }
            }

            LightPairRotationEventEffect lightPairRotationEvent = root.GetComponent<LightPairRotationEventEffect>();
            if (lightPairRotationEvent != null)
            {
                LightPairRotationEventEffect originalLightPairRotationEvent = original.GetComponent<LightPairRotationEventEffect>();

                _lightPairCallbackControllerAccessor(ref lightPairRotationEvent) = _lightPairCallbackControllerAccessor(ref originalLightPairRotationEvent);

                Transform transformL = _transformLAccessor(ref originalLightPairRotationEvent);
                Transform transformR = _transformRAccessor(ref originalLightPairRotationEvent);

                _transformLAccessor(ref lightPairRotationEvent) = root.GetChild(transformL.GetSiblingIndex());
                _transformRAccessor(ref lightPairRotationEvent) = root.GetChild(transformR.GetSiblingIndex());

                // We have to enable the object to tell unity to run Start
                lightPairRotationEvent.enabled = true;

                if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                {
                    ChromaLogger.Log($"Initialized LightPairRotationEventEffect");
                }
            }

            ParticleSystemEventEffect particleSystemEvent = root.GetComponent<ParticleSystemEventEffect>();
            if (particleSystemEvent != null)
            {
                ParticleSystemEventEffect originalParticleSystemEvent = original.GetComponent<ParticleSystemEventEffect>();

                _particleCallbackControllerAccessor(ref particleSystemEvent) = _particleCallbackControllerAccessor(ref originalParticleSystemEvent);
                _particleSystemAccessor(ref particleSystemEvent) = root.GetComponent<ParticleSystem>();

                particleSystemEvent.enabled = true;

                if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                {
                    ChromaLogger.Log($"Initialized ParticleSystemEventEffect");
                }
            }

            Mirror mirror = root.GetComponent<Mirror>();
            if (mirror != null)
            {
                _mirrorRendererAccessor(ref mirror) = Object.Instantiate(_mirrorRendererAccessor(ref mirror));
                _mirrorMaterialAccessor(ref mirror) = Object.Instantiate(_mirrorMaterialAccessor(ref mirror));

                if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                {
                    ChromaLogger.Log($"Initialized Mirror");
                }
            }

            GameObjectInfo newGameObjectInfo = new GameObjectInfo(root.gameObject);
            gameObjectInfos.Add(newGameObjectInfo);

            foreach (Transform transform in root)
            {
                int index = transform.GetSiblingIndex();
                InitializeComponents(transform, original.GetChild(index), gameObjectInfos, componentDatas);
            }
        }
    }
}
