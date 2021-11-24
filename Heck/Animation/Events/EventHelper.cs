using CustomJSONData.CustomBeatmap;
using static Heck.Animation.AnimationController;
using static Heck.HeckCustomDataManager;

namespace Heck.Animation.Events
{
    internal enum EventType
    {
        AnimateTrack,
        AssignPathAnimation
    }

    internal static class EventHelper
    {
        internal static void StartEventCoroutine(CustomEventData customEventData, EventType eventType)
        {
            HeckCoroutineEventData? heckData = TryGetEventData<HeckCoroutineEventData>(customEventData);
            if (heckData == null)
            {
                return;
            }

            float duration = heckData.Duration;
            duration = 60f * duration / AnimationController.BeatmapObjectSpawnController.currentBpm; // Convert to real time;

            Functions easing = heckData.Easing;

            foreach (HeckCoroutineEventData.CoroutineInfo coroutineInfo in heckData.CoroutineInfos)
            {
                Property property = coroutineInfo.Property;
                PointDefinition? pointData = coroutineInfo.PointDefinition;

                if (property.Coroutine != null)
                {
                    Instance.StopCoroutine(property.Coroutine);
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
                            property.Coroutine = Instance.StartCoroutine(AnimateTrack.AnimateTrackCoroutine(pointData, property, duration, customEventData.time, easing));
                            break;

                        case EventType.AssignPathAnimation:
                            ((PathProperty)property).Interpolation.Init(pointData);
                            property.Coroutine = Instance.StartCoroutine(AssignPathAnimation.AssignPathAnimationCoroutine(property, duration, customEventData.time, easing));
                            break;
                    }
                }
            }
        }
    }
}
