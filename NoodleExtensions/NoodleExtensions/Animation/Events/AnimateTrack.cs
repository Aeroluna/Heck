namespace NoodleExtensions.Animation
{
    using System.Collections;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;
    using static NoodleExtensions.Animation.AnimationController;

    internal class AnimateTrack
    {
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "AnimateTrack")
            {
                EventHelper.StartEventCoroutine(customEventData, EventType.AnimateTrack);
            }
        }

        internal static IEnumerator AnimateTrackCoroutine(PointDefinition points, Property property, float duration, float startTime, Functions easing)
        {
            while (true)
            {
                float elapsedTime = Instance.CustomEventCallbackController._audioTimeSource.songTime - startTime;
                float time = Easings.Interpolate(Mathf.Min(elapsedTime / duration, 1f), easing);
                switch (property.PropertyType)
                {
                    case PropertyType.Linear:
                        property.Value = points.InterpolateLinear(time);
                        break;

                    case PropertyType.Vector3:
                        property.Value = points.Interpolate(time);
                        break;

                    case PropertyType.Quaternion:
                        property.Value = points.InterpolateQuaternion(time);
                        break;
                }

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
