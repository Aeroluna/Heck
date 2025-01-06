using System.Collections.Generic;
using System.Linq;
using Chroma.HarmonyPatches.Colorizer.Initialize;
using Chroma.HarmonyPatches.EnvironmentComponent;
using Chroma.Settings;
using HarmonyLib;
using Heck.Animation.Transform;
using JetBrains.Annotations;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Chroma.EnvironmentEnhancement.Component;

internal class DuplicateInitializer
{
    private readonly SiraLog _log;
    private readonly DiContainer _container;
    private readonly TrackLaneRingOffset _trackLaneRingOffset;
    private readonly LightWithIdRegisterer _lightWithIdRegisterer;
    private readonly Config _config;
    private readonly HashSet<TrackLaneRingsManager> _trackLaneRingsManagers;

    [UsedImplicitly]
    private DuplicateInitializer(
        SiraLog log,
        DiContainer container,
        TrackLaneRingOffset trackLaneRingOffset,
        LightWithIdRegisterer lightWithIdRegisterer,
        Config config)
    {
        _log = log;
        _container = container;
        _trackLaneRingOffset = trackLaneRingOffset;
        _lightWithIdRegisterer = lightWithIdRegisterer;
        _config = config;
        _trackLaneRingsManagers = Resources.FindObjectsOfTypeAll<TrackLaneRingsManager>().ToHashSet();
    }

    internal static void PrefillComponentsData(Transform root, List<IComponentData> componentDatas)
    {
        TrackLaneRingsManager trackLaneRingsManager = root.GetComponent<TrackLaneRingsManager>();
        if (trackLaneRingsManager != null)
        {
            componentDatas.Add(
                new TrackLaneRingsManagerComponentData
                {
                    OldTrackLaneRingsManager = trackLaneRingsManager
                });
        }

        foreach (Transform transform in root)
        {
            PrefillComponentsData(transform, componentDatas);
        }
    }

    internal void InitializeComponents(
        Transform root,
        Transform original,
        List<GameObjectInfo> gameObjectInfos,
        List<IComponentData> componentDatas)
    {
        MonoBehaviour[] rootComponents = root.GetComponents<MonoBehaviour>();
        MonoBehaviour[] otherComponents = original.GetComponents<MonoBehaviour>();
        for (int i = 0; i < rootComponents.Length; i++)
        {
            MonoBehaviour monoBehaviour = rootComponents[i];
            MonoBehaviour other = otherComponents[i];

            _container.Inject(monoBehaviour);

            switch (monoBehaviour)
            {
                case TransformController transformController:
                    Object.DestroyImmediate(transformController);
                    break;

                case LightWithIdMonoBehaviour lightWithIdMonoBehaviour:
                    _lightWithIdRegisterer.MarkForTableRegister(lightWithIdMonoBehaviour);
                    break;

                case LightWithIds lightsWithIds:
                    foreach (LightWithIds.LightWithId light in lightsWithIds._lightWithIds)
                    {
                        _lightWithIdRegisterer.MarkForTableRegister(light);
                    }

                    break;

                case TrackLaneRing trackLaneRing:
                    TrackLaneRing originalTrackLaneRing = (TrackLaneRing)other;
                    _trackLaneRingOffset.CopyRing(originalTrackLaneRing, trackLaneRing);

                    trackLaneRing._transform = root;
                    trackLaneRing._positionOffset = originalTrackLaneRing._positionOffset;
                    trackLaneRing._posZ = originalTrackLaneRing._posZ;

                    TrackLaneRingsManager? managerToAdd = null;
                    foreach (TrackLaneRingsManager manager in _trackLaneRingsManagers)
                    {
                        TrackLaneRingsManagerComponentData? componentData = componentDatas
                            .OfType<TrackLaneRingsManagerComponentData>()
                            .FirstOrDefault(n => n.OldTrackLaneRingsManager == manager);
                        if (componentData != null)
                        {
                            managerToAdd = componentData.NewTrackLaneRingsManager;
                        }
                        else
                        {
                            TrackLaneRing[] rings = manager._rings;
                            if (rings.Contains(originalTrackLaneRing))
                            {
                                managerToAdd = manager;
                            }
                        }

                        // ReSharper disable once InvertIf
                        if (managerToAdd != null)
                        {
                            managerToAdd._rings = managerToAdd._rings.AddToArray(trackLaneRing);

                            break;
                        }
                    }

                    break;

                case TrackLaneRingsPositionStepEffectSpawner trackLaneRingsPositionStepEffectSpawner:
                    foreach (TrackLaneRingsManager manager in _trackLaneRingsManagers)
                    {
                        TrackLaneRingsManagerComponentData? componentData = componentDatas
                            .OfType<TrackLaneRingsManagerComponentData>()
                            .FirstOrDefault(n => n.OldTrackLaneRingsManager == manager);
                        if (componentData == null)
                        {
                            continue;
                        }

                        trackLaneRingsPositionStepEffectSpawner._trackLaneRingsManager = componentData.NewTrackLaneRingsManager!;

                        break;
                    }

                    break;

                case TrackLaneRingsRotationEffectSpawner trackLaneRingsRotationEffectSpawner:
                    trackLaneRingsRotationEffectSpawner._trackLaneRingsRotationEffect =
                        root.GetComponent<TrackLaneRingsRotationEffect>();

                    break;

                case Spectrogram spectrogram:
                    spectrogram._meshRenderers = root.GetComponentsInChildren<MeshRenderer>();

                    break;

                case LightPairRotationEventEffect lightPairRotationEventEffect:
                    LightPairRotationEventEffect originalLightPairRotationEventEffect = (LightPairRotationEventEffect)other;
                    Transform transformL = originalLightPairRotationEventEffect._transformL;
                    Transform transformR = originalLightPairRotationEventEffect._transformR;

                    lightPairRotationEventEffect._transformL = root.GetChild(transformL.GetSiblingIndex());
                    lightPairRotationEventEffect._transformR = root.GetChild(transformR.GetSiblingIndex());

                    // We have to enable the object to tell unity to run Start
                    lightPairRotationEventEffect.enabled = true;

                    break;

                case ParticleSystemEventEffect particleSystemEventEffect:
                    particleSystemEventEffect._particleSystem = root.GetComponent<ParticleSystem>();
                    particleSystemEventEffect.enabled = true;

                    break;

                case Mirror mirror:
                    mirror._renderer = root.GetComponent<MeshRenderer>();

                    break;
            }

            if (_config.PrintEnvironmentEnhancementDebug)
            {
                _log.Debug($"Initialized {monoBehaviour.GetType().Name}");
            }
        }

        GameObjectInfo newGameObjectInfo = new(root.gameObject);
        gameObjectInfos.Add(newGameObjectInfo);

        foreach (Transform transform in root)
        {
            int index = transform.GetSiblingIndex();
            InitializeComponents(transform, original.GetChild(index), gameObjectInfos, componentDatas);
        }
    }

    internal void PostfillComponentsData(Transform root, Transform original, List<IComponentData> componentDatas)
    {
        TrackLaneRingsManager trackLaneRingsManager = root.GetComponent<TrackLaneRingsManager>();
        if (trackLaneRingsManager != null)
        {
            _trackLaneRingsManagers.Add(trackLaneRingsManager);
            TrackLaneRingsManager originalManager = original.GetComponent<TrackLaneRingsManager>();
            foreach (TrackLaneRingsManagerComponentData componentData in componentDatas
                         .OfType<TrackLaneRingsManagerComponentData>()
                         .Where(n => n.OldTrackLaneRingsManager == originalManager))
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
}
