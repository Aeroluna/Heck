using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NoodleExtensions.Animation.AnimationController;
using static NoodleExtensions.Animation.AnimationHelper;
using static NoodleExtensions.Plugin;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;

namespace NoodleExtensions.Animation
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
            Track track = GetTrack(customEventData.data);
            if (track != null)
            {
                float duration = (float?)Trees.at(customEventData.data, DURATION) ?? 0f;
                duration = (60f * duration) / instance.beatmapObjectSpawnController.currentBPM; // Convert to real time;

                string easingString = (string)Trees.at(customEventData.data, EASING);
                Functions easing = Functions.easeLinear;
                if (easingString != null) easing = (Functions)Enum.Parse(typeof(Functions), easingString);

                List<string> excludedStrings = new List<string> { TRACK, DURATION, EASING };
                IDictionary<string, object> eventData = new Dictionary<string, object>(customEventData.data as IDictionary<string, object>); // Shallow copy
                IDictionary<string, Property> properties = null;
                switch (eventType)
                {
                    case EventType.AnimateTrack:
                        properties = track._properties;
                        break;
                    case EventType.AssignPathAnimation:
                        properties = track._pathProperties;
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
                            Logger.Log($"Could not find property {valuePair.Key}!", IPA.Logging.Logger.Level.Error);
                            continue;
                        }

                        TryGetPointData(customEventData.data, valuePair.Key, out PointData pointData);

                        if (property._coroutine != null) instance.StopCoroutine(property._coroutine);
                        switch (eventType)
                        {
                            case EventType.AnimateTrack:
                                property._coroutine = instance.StartCoroutine(AnimateTrack.AnimateTrackCoroutine(pointData, property, duration, customEventData.time, easing));
                                break;
                            case EventType.AssignPathAnimation:
                                ((PointDataInterpolation)property._property).Init(pointData);
                                property._coroutine = instance.StartCoroutine(AssignPathAnimation.AssignPathAnimationCoroutine(property, duration, customEventData.time, easing));
                                break;
                        }
                    }
                }
            }
        }
    }
}
