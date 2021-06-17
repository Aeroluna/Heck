namespace Chroma
{
    using System;
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
                UnityEngine.Object.Destroy(rotationEffect);
            }

            foreach (Transform transform in root)
            {
                int index = transform.GetSiblingIndex();
                PostfillComponentsData(transform, original.GetChild(index), componentDatas);
            }
        }

        internal static void InitializeComponents(Transform root, Transform original, List<GameObjectInfo> gameObjectInfos, List<IComponentData> componentDatas, int? lightID)
        {
            void GetComponentAndOriginal<T>(Action<T, T> initializeDelegate)
            {
                T[] rootComponents = root.GetComponents<T>();
                T[] originalComponents = original.GetComponents<T>();

                for (int i = 0; i < rootComponents.Length; i++)
                {
                    initializeDelegate(rootComponents[i], originalComponents[i]);

                    if (Settings.ChromaConfig.Instance.PrintEnvironmentEnhancementDebug)
                    {
                        Plugin.Logger.Log($"Initialized {typeof(T).Name}");
                    }
                }
            }

            GetComponentAndOriginal<LightWithIdMonoBehaviour>((rootComponent, originalComponent) =>
            {
                _lightWithIdMonoBehaviourManagerAccessor(ref rootComponent) = _lightWithIdMonoBehaviourManagerAccessor(ref originalComponent);
                LightColorizer.RegisterLight(rootComponent, lightID);
            });

            GetComponentAndOriginal<LightWithIds>((rootComponent, originalComponent) =>
            {
                _lightWithIdsManagerAccessor(ref rootComponent) = _lightWithIdsManagerAccessor(ref originalComponent);
                LightColorizer.RegisterLight(rootComponent, lightID);
            });

            GetComponentAndOriginal<TrackLaneRing>((rootComponent, originalComponent) =>
            {
                if (EnvironmentEnhancementManager.RingRotationOffsets.TryGetValue(originalComponent, out Quaternion offset))
                {
                    EnvironmentEnhancementManager.RingRotationOffsets.Add(rootComponent, offset);
                }

                _ringTransformAccessor(ref rootComponent) = root;
                _positionOffsetAccessor(ref rootComponent) = _positionOffsetAccessor(ref originalComponent);
                _posZAccessor(ref rootComponent) = _posZAccessor(ref originalComponent);

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
                        if (rings.Contains(originalComponent))
                        {
                            managerToAdd = manager;
                        }
                    }

                    if (managerToAdd != null)
                    {
                        // ToList() to add and then back ToArray()
                        TrackLaneRing[] rings = _ringsAccessor(ref managerToAdd);
                        List<TrackLaneRing> ringsList = rings?.ToList() ?? new List<TrackLaneRing>();
                        ringsList.Add(rootComponent);
                        _ringsAccessor(ref managerToAdd) = ringsList.ToArray();

                        break;
                    }
                }
            });

            GetComponentAndOriginal<TrackLaneRingsPositionStepEffectSpawner>((rootComponent, originalComponent) =>
            {
                foreach (TrackLaneRingsManager manager in HarmonyPatches.TrackLaneRingsManagerAwake.RingManagers)
                {
                    TrackLaneRingsManagerComponentData componentData = componentDatas.OfType<TrackLaneRingsManagerComponentData>().Where(n => n.OldTrackLaneRingsManager == manager).FirstOrDefault();
                    if (componentData != null)
                    {
                        _stepSpawnerRingsManagerAccessor(ref rootComponent) = componentData.NewTrackLaneRingsManager;

                        break;
                    }
                }
            });

            GetComponentAndOriginal<ChromaRingsRotationEffect>((rootComponent, originalComponent) =>
            {
                foreach (TrackLaneRingsManager manager in HarmonyPatches.TrackLaneRingsManagerAwake.RingManagers)
                {
                    TrackLaneRingsManagerComponentData componentData = componentDatas.OfType<TrackLaneRingsManagerComponentData>().Where(n => n.OldTrackLaneRingsManager == manager).FirstOrDefault();
                    if (componentData != null)
                    {
                        rootComponent.SetNewRingManager(componentData.NewTrackLaneRingsManager);

                        break;
                    }
                }
            });

            GetComponentAndOriginal<TrackLaneRingsRotationEffectSpawner>((rootComponent, originalComponent) =>
            {
                _rotationEffectSpawnerCallbackControllerAccessor(ref rootComponent) = _rotationEffectSpawnerCallbackControllerAccessor(ref originalComponent);
                _trackLaneRingsRotationEffectAccessor(ref rootComponent) = rootComponent.GetComponent<ChromaRingsRotationEffect>();
            });

            GetComponentAndOriginal<Spectrogram>((rootComponent, originalComponent) => _spectrogramDataAccessor(ref rootComponent) = _spectrogramDataAccessor(ref originalComponent));

            GetComponentAndOriginal<LightRotationEventEffect>((rootComponent, originalComponent) => _lightCallbackControllerAccessor(ref rootComponent) = _lightCallbackControllerAccessor(ref originalComponent));

            GetComponentAndOriginal<LightPairRotationEventEffect>((rootComponent, originalComponent) =>
            {
                _lightPairCallbackControllerAccessor(ref rootComponent) = _lightPairCallbackControllerAccessor(ref originalComponent);

                Transform transformL = _transformLAccessor(ref originalComponent);
                Transform transformR = _transformRAccessor(ref originalComponent);

                _transformLAccessor(ref rootComponent) = root.GetChild(transformL.GetSiblingIndex());
                _transformRAccessor(ref rootComponent) = root.GetChild(transformR.GetSiblingIndex());

                // We have to enable the object to tell unity to run Start
                rootComponent.enabled = true;
            });

            GetComponentAndOriginal<ParticleSystemEventEffect>((rootComponent, originalComponent) =>
            {
                _particleCallbackControllerAccessor(ref rootComponent) = _particleCallbackControllerAccessor(ref originalComponent);
                _particleSystemAccessor(ref rootComponent) = root.GetComponent<ParticleSystem>();

                rootComponent.enabled = true;
            });

            GetComponentAndOriginal<Mirror>((rootComponent, originalComponent) =>
            {
                _mirrorRendererAccessor(ref rootComponent) = UnityEngine.Object.Instantiate(_mirrorRendererAccessor(ref originalComponent));
                _mirrorMaterialAccessor(ref rootComponent) = UnityEngine.Object.Instantiate(_mirrorMaterialAccessor(ref originalComponent));
            });

            GameObjectInfo newGameObjectInfo = new GameObjectInfo(root.gameObject);
            gameObjectInfos.Add(newGameObjectInfo);

            foreach (Transform transform in root)
            {
                int index = transform.GetSiblingIndex();
                InitializeComponents(transform, original.GetChild(index), gameObjectInfos, componentDatas, lightID);
            }
        }
    }
}
