using System.Collections;
using CustomJSONData.CustomBeatmap;
using UnityEngine;
using static Heck.Animation.AnimationController;
using static Heck.HeckController;

namespace Heck.Animation.Events
{
    internal static class AnimateTrack
    {
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == ANIMATE_TRACK)
            {
                EventHelper.StartEventCoroutine(customEventData, EventType.AnimateTrack);
            }
        }

        internal static IEnumerator AnimateTrackCoroutine(PointDefinition points, Property property, float duration, float startTime, Functions easing)
        {
            while (true)
            {
                float elapsedTime = Instance.CustomEventCallbackController.AudioTimeSource!.songTime - startTime;
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
    }
}
