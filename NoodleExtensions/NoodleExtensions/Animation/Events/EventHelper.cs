namespace NoodleExtensions.Animation
{
    using CustomJSONData.CustomBeatmap;
    using static NoodleExtensions.Animation.AnimationController;
    using static NoodleExtensions.Animation.NoodleEventDataManager;

    internal enum EventType
    {
        AnimateTrack,
        AssignPathAnimation,
    }

    internal static class EventHelper
    {
        internal static void StartEventCoroutine(CustomEventData customEventData, EventType eventType)
        {
            NoodleCoroutineEventData noodleData = TryGetEventData<NoodleCoroutineEventData>(customEventData);
            if (noodleData == null)
            {
                return;
            }

            float duration = noodleData.Duration;
            duration = 60f * duration / Instance.BeatmapObjectSpawnController.currentBpm; // Convert to real time;

            Functions easing = noodleData.Easing;

            foreach (NoodleCoroutineEventData.CoroutineInfo coroutineInfo in noodleData.CoroutineInfos)
            {
                Property property = coroutineInfo.Property;
                PointDefinition pointData = coroutineInfo.PointDefinition;

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
                            ((PointDefinitionInterpolation)property.Value).Init(null);
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
                            ((PointDefinitionInterpolation)property.Value).Init(pointData);
                            property.Coroutine = Instance.StartCoroutine(AssignPathAnimation.AssignPathAnimationCoroutine(property, duration, customEventData.time, easing));
                            break;
                    }
                }
            }
        }
    }
}
