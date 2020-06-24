using CustomJSONData.CustomBeatmap;
using System.Collections;
using UnityEngine;
using static NoodleExtensions.Animation.AnimationController;

namespace NoodleExtensions.Animation
{
    internal class AnimateTrack
    {
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "AnimateTrack")
            {
                EventHelper.StartEventCoroutine(customEventData, EventType.AnimateTrack);
            }
        }

        internal static IEnumerator AnimateTrackCoroutine(PointData points, Property property, float duration, float startTime, Functions easing)
        {
            while (true)
            {
                float elapsedTime = instance.customEventCallbackController._audioTimeSource.songTime - startTime;
                float time = Easings.Interpolate(Mathf.Min(elapsedTime / duration, 1f), easing);
                switch (property._propertyType)
                {
                    case PropertyType.Linear:
                        property._property = points.InterpolateLinear(time);
                        break;

                    case PropertyType.Vector3:
                        property._property = points.Interpolate(time);
                        break;

                    case PropertyType.Quaternion:
                        property._property = points.InterpolateQuaternion(time);
                        break;
                }

                if (elapsedTime < duration) yield return null;
                else break;
            }
        }
    }
}
