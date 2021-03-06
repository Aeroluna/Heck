using System;
using System.Collections;
using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static Heck.HeckController;

namespace Heck.Animation.Events
{
    internal enum EventType
    {
        AnimateTrack,
        AssignPathAnimation
    }

    internal class CoroutineEventManager
    {
        private readonly IBpmController _bpmController;
        private readonly IAudioTimeSource _audioTimeSource;
        private readonly CoroutineDummy _coroutineDummy;
        private readonly DeserializedData _deserializedData;

        [UsedImplicitly]
        private CoroutineEventManager(
            IBpmController bpmController,
            IAudioTimeSource audioTimeSource,
            CoroutineDummy coroutineDummy,
            [Inject(Id = ID)] DeserializedData deserializedData)
        {
            _bpmController = bpmController;
            _audioTimeSource = audioTimeSource;
            _coroutineDummy = coroutineDummy;
            _deserializedData = deserializedData;
        }

        internal void StartEventCoroutine(CustomEventData customEventData, EventType eventType)
        {
            if (!_deserializedData.Resolve(customEventData, out HeckCoroutineEventData? heckData))
            {
                return;
            }

            float duration = 60f * heckData.Duration / _bpmController.currentBpm; // Convert to real time;
            Functions easing = heckData.Easing;
            int repeat = heckData.Repeat;
            bool noDuration = duration == 0 || customEventData.time + duration < _audioTimeSource.songTime;
            foreach (HeckCoroutineEventData.CoroutineInfo coroutineInfo in heckData.CoroutineInfos)
            {
                Property property = coroutineInfo.Property;
                PointDefinition? pointData = coroutineInfo.PointDefinition;

                if (property.Coroutine != null)
                {
                    _coroutineDummy.StopCoroutine(property.Coroutine);
                }

                if (pointData == null)
                {
                    switch (eventType)
                    {
                        case EventType.AnimateTrack:
                            coroutineInfo.Track.UpdatedThisFrame = true;
                            property.LinearValue = null;
                            property.QuaternionValue = null;
                            property.Vector3Value = null;
                            property.Vector4Value = null;
                            break;

                        case EventType.AssignPathAnimation:
                            ((PathProperty)property).Interpolation.Init(null);
                            break;
                    }
                }
                else
                {
                    switch (eventType)
                    {
                        case EventType.AnimateTrack:
                            if (noDuration)
                            {
                                SetPropertyValue(pointData, coroutineInfo.Track, property, 1, out _);
                            }
                            else
                            {
                                property.Coroutine = _coroutineDummy.StartCoroutine(AnimateTrackCoroutine(
                                    pointData,
                                    coroutineInfo.Track,
                                    property,
                                    duration,
                                    customEventData.time,
                                    easing,
                                    repeat));
                            }

                            break;

                        case EventType.AssignPathAnimation:
                            PathProperty pathProperty = (PathProperty)property;
                            pathProperty.Interpolation.Init(pointData);
                            if (noDuration)
                            {
                                pathProperty.Interpolation.Finish();
                            }
                            else
                            {
                                property.Coroutine = _coroutineDummy.StartCoroutine(AssignPathAnimationCoroutine(
                                    property,
                                    duration,
                                    customEventData.time,
                                    easing));
                            }

                            break;
                    }
                }
            }
        }

        private static void SetPropertyValue(
            PointDefinition points,
            Track track,
            Property property,
            float time,
            out bool onLast)
        {
            switch (property.PropertyType)
            {
                case PropertyType.Linear:
                    float value = points.InterpolateLinear(time, out onLast);

                    if (!property.LinearValue.HasValue ||
                        !Mathf.Approximately(property.LinearValue.Value, value))
                    {
                        property.LinearValue = value;
                        track.UpdatedThisFrame = true;
                    }

                    break;

                case PropertyType.Quaternion:
                    Quaternion quaternion = points.InterpolateQuaternion(time, out onLast);
                    if (!property.QuaternionValue.HasValue ||
                        Quaternion.Dot(property.QuaternionValue.Value, quaternion) < 1)
                    {
                        property.QuaternionValue = quaternion;
                        track.UpdatedThisFrame = true;
                    }

                    break;

                case PropertyType.Vector3:
                    Vector3 vector = points.Interpolate(time, out onLast);
                    if (property.Vector3Value != vector)
                    {
                        property.Vector3Value = vector;
                        track.UpdatedThisFrame = true;
                    }

                    break;

                case PropertyType.Vector4:
                    Vector4 vector4 = points.InterpolateVector4(time, out onLast);
                    if (property.Vector4Value != vector4)
                    {
                        property.Vector4Value = vector4;
                        track.UpdatedThisFrame = true;
                    }

                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private IEnumerator AnimateTrackCoroutine(
            PointDefinition points,
            Track track,
            Property property,
            float duration,
            float startTime,
            Functions easing,
            int repeat)
        {
            bool onLast = false;
            while (repeat >= 0)
            {
                float elapsedTime = _audioTimeSource.songTime - startTime;
                if (!onLast)
                {
                    float normalizedTime = Mathf.Min(elapsedTime / duration, 1);
                    float time = Easings.Interpolate(normalizedTime, easing);
                    SetPropertyValue(points, track, property, time, out onLast);
                }

                if (elapsedTime < duration)
                {
                    if (repeat <= 0 && onLast)
                    {
                        break;
                    }

                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    repeat--;
                    startTime += duration;
                    onLast = false;
                }
            }
        }

        private IEnumerator AssignPathAnimationCoroutine(
            Property property,
            float duration,
            float startTime,
            Functions easing)
        {
            PointDefinitionInterpolation pointDataInterpolation = ((PathProperty)property).Interpolation;
            float elapsedTime;
            do
            {
                elapsedTime = _audioTimeSource.songTime - startTime;
                float normalizedTime = Mathf.Min(elapsedTime / duration, 1);
                pointDataInterpolation.Time = Easings.Interpolate(normalizedTime, easing);
                yield return null;
            }
            while (elapsedTime < duration);

            pointDataInterpolation.Finish();
        }
    }
}
