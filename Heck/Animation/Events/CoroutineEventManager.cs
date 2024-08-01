using System;
using System.Collections;
using CustomJSONData.CustomBeatmap;
using Heck.Deserialize;
using Heck.Event;
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

    [CustomEvent(ANIMATE_TRACK, ASSIGN_PATH_ANIMATION)]
    internal class CoroutineEvent : ICustomEvent
    {
        private readonly IBpmController _bpmController;
        private readonly IAudioTimeSource _audioTimeSource;
        private readonly CoroutineDummy _coroutineDummy;
        private readonly DeserializedData _deserializedData;

        [UsedImplicitly]
        private CoroutineEvent(
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

        public void Callback(CustomEventData customEventData)
        {
            switch (customEventData.eventType)
            {
                case ANIMATE_TRACK:
                    StartEventCoroutine(customEventData, EventType.AnimateTrack);
                    break;
                case ASSIGN_PATH_ANIMATION:
                    StartEventCoroutine(customEventData, EventType.AssignPathAnimation);
                    break;

                // TODO: reimplement this
                /*case INVOKE_EVENT:
                    if (_customData.Resolve(customEventData, out HeckInvokeEventData? heckData))
                    {
                        _customEventCallbackController.InvokeCustomEvent(heckData.CustomEventData);
                    }

                    break;*/
            }
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
            bool noDuration = duration == 0 || customEventData.time + (duration * (repeat + 1)) < _audioTimeSource.songTime;
            foreach (HeckCoroutineEventData.CoroutineInfo coroutineInfo in heckData.CoroutineInfos)
            {
                BaseProperty property = coroutineInfo.Property;
                IPointDefinition? pointData = coroutineInfo.PointDefinition;

                if (property.Coroutine != null)
                {
                    _coroutineDummy.StopCoroutine(property.Coroutine);
                }

                if (pointData == null)
                {
                    coroutineInfo.Track.UpdatedThisFrame = true;
                    property.Null();
                }
                else
                {
                    bool hasBase = pointData.HasBaseProvider;
                    switch (eventType)
                    {
                        case EventType.AnimateTrack:
                            if (noDuration || (pointData.Count <= 1 && !hasBase))
                            {
                                SetPropertyValue(pointData, property, coroutineInfo.Track, 1, out _);
                            }
                            else
                            {
                                property.Coroutine = _coroutineDummy.StartCoroutine(AnimateTrackCoroutine(
                                    pointData,
                                    property,
                                    coroutineInfo.Track,
                                    duration,
                                    customEventData.time,
                                    easing,
                                    repeat,
                                    hasBase));
                            }

                            break;

                        case EventType.AssignPathAnimation:
                            IPointDefinitionInterpolation interpolation = ((BasePathProperty)property).IInterpolation;
                            interpolation.Init(pointData);
                            if (noDuration)
                            {
                                interpolation.Finish();
                            }
                            else
                            {
                                property.Coroutine = _coroutineDummy.StartCoroutine(AssignPathAnimationCoroutine(
                                    interpolation,
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
            IPointDefinition points,
            BaseProperty property,
            Track track,
            float time,
            out bool onLast)
        {
            switch (points)
            {
                case PointDefinition<float> values:
                    SetPropertyValue(values, Cast<float>(property), track, time, out onLast);
                    break;

                case PointDefinition<Vector3> values:
                    SetPropertyValue(values, Cast<Vector3>(property), track, time, out onLast);
                    break;

                case PointDefinition<Vector4> values:
                    SetPropertyValue(values, Cast<Vector4>(property), track, time, out onLast);
                    break;

                case PointDefinition<Quaternion> values:
                    SetPropertyValue(values, Cast<Quaternion>(property), track, time, out onLast);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(points));
            }

            return;

            static Property<T> Cast<T>(BaseProperty toCast)
                where T : struct
            {
                return toCast as Property<T> ?? throw new InvalidOperationException();
            }
        }

        private static void SetPropertyValue(
            PointDefinition<float> points,
            Property<float> property,
            Track track,
            float time,
            out bool onLast)
        {
            float value = points.Interpolate(time, out onLast);
            if (property.Value.HasValue && property.Value.Value.EqualsTo(value))
            {
                return;
            }

            property.Value = value;
            track.UpdatedThisFrame = true;
        }

        private static void SetPropertyValue(
            PointDefinition<Vector3> points,
            Property<Vector3> property,
            Track track,
            float time,
            out bool onLast)
        {
            Vector3 value = points.Interpolate(time, out onLast);
            if (property.Value.HasValue && property.Value.Value.EqualsTo(value))
            {
                return;
            }

            property.Value = value;
            track.UpdatedThisFrame = true;
        }

        private static void SetPropertyValue(
            PointDefinition<Vector4> points,
            Property<Vector4> property,
            Track track,
            float time,
            out bool onLast)
        {
            Vector4 value = points.Interpolate(time, out onLast);
            if (property.Value.HasValue && property.Value.Value.EqualsTo(value))
            {
                return;
            }

            property.Value = value;
            track.UpdatedThisFrame = true;
        }

        private static void SetPropertyValue(
            PointDefinition<Quaternion> points,
            Property<Quaternion> property,
            Track track,
            float time,
            out bool onLast)
        {
            Quaternion value = points.Interpolate(time, out onLast);
            if (property.Value.HasValue &&
                property.Value.Value.EqualsTo(value))
            {
                return;
            }

            property.Value = value;
            track.UpdatedThisFrame = true;
        }

        private IEnumerator AnimateTrackCoroutine(
            IPointDefinition points,
            BaseProperty property,
            Track track,
            float duration,
            float startTime,
            Functions easing,
            int repeat,
            bool nonLazy)
        {
            bool skip = false;
            WaitForEndOfFrame waitForEndOfFrame = new();
            while (repeat >= 0)
            {
                float elapsedTime = _audioTimeSource.songTime - startTime;
                if (!skip)
                {
                    float normalizedTime = Mathf.Min(elapsedTime / duration, 1);
                    float time = Easings.Interpolate(normalizedTime, easing);
                    SetPropertyValue(points, property, track, time, out bool onLast);
                    skip = !nonLazy && onLast;
                }

                if (elapsedTime < duration)
                {
                    if (repeat <= 0 && skip)
                    {
                        break;
                    }

                    yield return waitForEndOfFrame;
                }
                else
                {
                    repeat--;
                    startTime += duration;
                    skip = false;
                }
            }
        }

        private IEnumerator AssignPathAnimationCoroutine(
            IPointDefinitionInterpolation interpolation,
            float duration,
            float startTime,
            Functions easing)
        {
            float elapsedTime;
            do
            {
                elapsedTime = _audioTimeSource.songTime - startTime;
                float normalizedTime = Mathf.Min(elapsedTime / duration, 1);
                interpolation.Time = Easings.Interpolate(normalizedTime, easing);
                yield return null;
            }
            while (elapsedTime < duration);

            interpolation.Finish();
        }
    }
}
