using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chroma.EnvironmentEnhancement.Component;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using Heck.Animation;
using Heck.Deserialize;
using Heck.Event;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static Chroma.ChromaController;
using static Chroma.EnvironmentEnhancement.Component.ComponentConstants;

namespace Chroma.Animation;

[CustomEvent(ANIMATE_COMPONENT)]
internal class AnimateComponent : ICustomEvent
{
    private readonly Dictionary<string, Dictionary<Track, Coroutine>> _allCoroutines = new();
    private readonly IBpmController _bpmController;
    private readonly BeatmapCallbacksController _beatmapCallbacksController;
    private readonly CoroutineDummy _coroutineDummy;
    private readonly DeserializedData _deserializedData;

    [UsedImplicitly]
    private AnimateComponent(
        IBpmController bpmController,
        BeatmapCallbacksController beatmapCallbacksController,
        CoroutineDummy coroutineDummy,
        [Inject(Id = ID)] DeserializedData deserializedData)
    {
        _bpmController = bpmController;
        _beatmapCallbacksController = beatmapCallbacksController;
        _coroutineDummy = coroutineDummy;
        _deserializedData = deserializedData;
    }

    public void Callback(CustomEventData customEventData)
    {
        if (!_deserializedData.Resolve(customEventData, out ChromaAnimateComponentData? chromaData))
        {
            return;
        }

        float duration = chromaData.Duration;
        duration = (60f * duration) / _bpmController.currentBpm; // Convert to real time;

        Functions easing = chromaData.Easing;
        IReadOnlyList<Track> tracks = chromaData.Track;

        foreach ((string componentName, Dictionary<string, PointDefinition<float>?> properties) in chromaData
                     .CoroutineInfos)
        {
            foreach (Track track in tracks)
            {
                object[] components;

                switch (componentName)
                {
                    case BLOOM_FOG_ENVIRONMENT:
                        components = BloomFogCustomizer.GetComponents(track);
                        if (components.Length == 0)
                        {
                            break;
                        }

                        HandleProperty<BloomFogEnvironmentParams>(ATTENUATION, (a, b) => a.Do(n => n.attenuation = b));
                        HandleProperty<BloomFogEnvironmentParams>(OFFSET, (a, b) => a.Do(n => n.offset = b));
                        HandleProperty<BloomFogEnvironmentParams>(
                            HEIGHT_FOG_HEIGHT,
                            (a, b) => a.Do(n => n.heightFogHeight = b));
                        HandleProperty<BloomFogEnvironmentParams>(
                            HEIGHT_FOG_STARTY,
                            (a, b) => a.Do(n => n.heightFogStartY = b));
                        break;

                    case TUBE_BLOOM_PRE_PASS_LIGHT:
                        components = TubeBloomLightCustomizer.GetComponents(track);
                        if (components.Length == 0)
                        {
                            break;
                        }

                        HandleProperty<TubeBloomPrePassLight>(
                            COLOR_ALPHA_MULTIPLIER,
                            (a, b) => a.Do(n => TubeBloomLightCustomizer.SetColorAlphaMultiplier(n, b)));
                        HandleProperty<TubeBloomPrePassLight>(
                            BLOOM_FOG_INTENSITY_MULTIPLIER,
                            (a, b) => a.Do(n => n.bloomFogIntensityMultiplier = b));
                        break;
                }

                continue;

                void HandleProperty<T>(string key, Action<T[], float> action)
                {
                    if (!properties.TryGetValue(key, out PointDefinition<float>? points))
                    {
                        return;
                    }

                    if (!_allCoroutines.TryGetValue(key, out Dictionary<Track, Coroutine> coroutines))
                    {
                        coroutines = new Dictionary<Track, Coroutine>();
                        _allCoroutines[key] = coroutines;
                    }

                    if (coroutines.TryGetValue(track, out Coroutine? coroutine))
                    {
                        if (coroutine != null)
                        {
                            _coroutineDummy.StopCoroutine(coroutine);
                        }
                    }

                    if (points == null)
                    {
                        return;
                    }

                    coroutines[track] = _coroutineDummy
                        .StartCoroutine(
                            AnimateCoroutine(
                                components.Cast<T>().ToArray(),
                                points,
                                duration,
                                customEventData.time,
                                easing,
                                action));
                }
            }
        }
    }

    private IEnumerator AnimateCoroutine<T>(
        T[] component,
        PointDefinition<float> points,
        float duration,
        float startTime,
        Functions easing,
        Action<T[], float> action)
    {
        while (true)
        {
            float elapsedTime = _beatmapCallbacksController.songTime - startTime;
            float time = Easings.Interpolate(Mathf.Min(elapsedTime / duration, 1f), easing);
            action(component, points.Interpolate(time));

            if (elapsedTime < duration)
            {
                yield return null;
            }
            else
            {
                break;
            }
        }
    }
}
