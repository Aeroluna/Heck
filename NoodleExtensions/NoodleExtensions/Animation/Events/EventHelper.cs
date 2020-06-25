namespace NoodleExtensions.Animation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using static NoodleExtensions.Animation.AnimationController;
    using static NoodleExtensions.Animation.AnimationHelper;
    using static NoodleExtensions.Plugin;

    internal enum EventType
    {
        AnimateTrack,
        AssignPathAnimation,
    }

    internal static class EventHelper
    {
        internal static void StartEventCoroutine(CustomEventData customEventData, EventType eventType)
        {
            Track track = GetTrack(customEventData.data);
            if (track != null)
            {
                float duration = (float?)Trees.at(customEventData.data, DURATION) ?? 0f;
                duration = (60f * duration) / Instance.BeatmapObjectSpawnController.currentBPM; // Convert to real time;

                string easingString = (string)Trees.at(customEventData.data, EASING);
                Functions easing = Functions.easeLinear;
                if (easingString != null)
                {
                    easing = (Functions)Enum.Parse(typeof(Functions), easingString);
                }

                List<string> excludedStrings = new List<string> { TRACK, DURATION, EASING };
                IDictionary<string, object> eventData = new Dictionary<string, object>(customEventData.data as IDictionary<string, object>); // Shallow copy
                IDictionary<string, Property> properties = null;
                switch (eventType)
                {
                    case EventType.AnimateTrack:
                        properties = track.Properties;
                        break;

                    case EventType.AssignPathAnimation:
                        properties = track.PathProperties;
                        break;

                    default:
                        return;
                }

                foreach (KeyValuePair<string, object> valuePair in eventData)
                {
                    if (!excludedStrings.Any(n => n == valuePair.Key))
                    {
                        Property property;
                        if (!properties.TryGetValue(valuePair.Key, out property))
                        {
                            NoodleLogger.Log($"Could not find property {valuePair.Key}!", IPA.Logging.Logger.Level.Error);
                            continue;
                        }

                        TryGetPointData(customEventData.data, valuePair.Key, out PointDefinition pointData);

                        if (property.Coroutine != null)
                        {
                            Instance.StopCoroutine(property.Coroutine);
                        }

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
}
