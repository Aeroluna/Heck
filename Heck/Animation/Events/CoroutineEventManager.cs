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
                            property.Value = null;
                            break;

                        case EventType.AssignPathAnimation:
                            ((PathProperty)property).Interpolation.Init(null);
                            break;
                    }
                }
                else
                {
                    // Avoid needless background running of the event if it's just a single point
                    // TODO: Implement Swifter's PointDefinition trimming optimization
                    float pointDuration = pointData.IsSingle() ? 0 : duration;

                    switch (eventType)
                    {
                        case EventType.AnimateTrack:
                            property.Coroutine = _coroutineDummy.StartCoroutine(AnimateTrackCoroutine(
                                pointData,
                                property,
                                pointDuration,
                                customEventData.time,
                                easing,
                                repeat));
                            break;

                        case EventType.AssignPathAnimation:
                            ((PathProperty)property).Interpolation.Init(pointData);
                            property.Coroutine = _coroutineDummy.StartCoroutine(AssignPathAnimationCoroutine(
                                property,
                                pointDuration,
                                customEventData.time,
                                easing));
                            break;
                    }
                }
            }
        }

        private IEnumerator AnimateTrackCoroutine(
            PointDefinition points,
            Property property,
            float duration,
            float startTime,
            Functions easing,
            int repeat)
        {
            while (repeat >= 0)
            {
                float elapsedTime = _audioTimeSource.songTime - startTime;
                float normalizedTime = duration > 0f ? Mathf.Min(elapsedTime / duration, 1) : 1f;
                float time = Easings.Interpolate(normalizedTime, easing);
                property.Value = property.PropertyType switch
                {
                    PropertyType.Linear => points.InterpolateLinear(time),
                    PropertyType.Vector3 => points.Interpolate(time),
                    PropertyType.Vector4 => points.InterpolateVector4(time),
                    PropertyType.Quaternion => points.InterpolateQuaternion(time),
                    _ => property.Value
                };

                if (duration > 0 && elapsedTime < duration)
                {
                    yield return null;
                }
                else
                {
                    repeat--;
                    startTime += duration;
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
                float normalizedTime = duration > 0f ? Mathf.Min(elapsedTime / duration, 1) : 1f;
                pointDataInterpolation.Time = Easings.Interpolate(normalizedTime, easing);
                if (duration > 0) {
                    yield return null;
                } else {
                    yield break;
                }
            }
            while (elapsedTime < duration);

            pointDataInterpolation.Finish();
        }
    }
}
