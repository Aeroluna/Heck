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
        private readonly EventController _eventController;
        private readonly IAudioTimeSource _audioTimeSource;
        private readonly CustomData _customData;

        [UsedImplicitly]
        private CoroutineEventManager(
            IBpmController bpmController,
            EventController eventController,
            IAudioTimeSource audioTimeSource,
            [Inject(Id = ID)] CustomData customData)
        {
            _bpmController = bpmController;
            _eventController = eventController;
            _audioTimeSource = audioTimeSource;
            _customData = customData;
        }

        internal void StartEventCoroutine(CustomEventData customEventData, EventType eventType)
        {
            if (!_customData.Resolve(customEventData, out HeckCoroutineEventData? heckData))
            {
                return;
            }

            float duration = heckData.Duration;
            duration = 60f * duration / _bpmController.currentBpm; // Convert to real time;

            Functions easing = heckData.Easing;

            foreach (HeckCoroutineEventData.CoroutineInfo coroutineInfo in heckData.CoroutineInfos)
            {
                Property property = coroutineInfo.Property;
                PointDefinition? pointData = coroutineInfo.PointDefinition;

                if (property.Coroutine != null)
                {
                    _eventController.StopCoroutine(property.Coroutine);
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
                    switch (eventType)
                    {
                        case EventType.AnimateTrack:
                            property.Coroutine = _eventController.StartCoroutine(AnimateTrackCoroutine(
                                pointData,
                                property,
                                duration,
                                customEventData.time,
                                easing));
                            break;

                        case EventType.AssignPathAnimation:
                            ((PathProperty)property).Interpolation.Init(pointData);
                            property.Coroutine = _eventController.StartCoroutine(AssignPathAnimationCoroutine(
                                property,
                                duration,
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
            Functions easing)
        {
            while (true)
            {
                float elapsedTime = _audioTimeSource.songTime - startTime;
                float time = Easings.Interpolate(Mathf.Min(elapsedTime / duration, 1f), easing);
                property.Value = property.PropertyType switch
                {
                    PropertyType.Linear => points.InterpolateLinear(time),
                    PropertyType.Vector3 => points.Interpolate(time),
                    PropertyType.Vector4 => points.InterpolateVector4(time),
                    PropertyType.Quaternion => points.InterpolateQuaternion(time),
                    _ => property.Value
                };

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

        private IEnumerator AssignPathAnimationCoroutine(
            Property property,
            float duration,
            float startTime,
            Functions easing)
        {
            PointDefinitionInterpolation pointDataInterpolation = ((PathProperty)property).Interpolation;
            while (true)
            {
                float elapsedTime = _audioTimeSource.songTime - startTime;
                pointDataInterpolation.Time = Easings.Interpolate(Mathf.Min(elapsedTime / duration, 1f), easing);

                if (elapsedTime < duration)
                {
                    yield return null;
                }
                else
                {
                    break;
                }
            }

            pointDataInterpolation.Finish();
        }
    }
}
