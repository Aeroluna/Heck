namespace Heck.Animation
{
    using System.Collections;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;
    using static Heck.Animation.AnimationController;
    using static Heck.Plugin;

    internal static class AssignPathAnimation
    {
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == ASSIGNPATHANIMATION)
            {
                EventHelper.StartEventCoroutine(customEventData, EventType.AssignPathAnimation);
            }
        }

        internal static IEnumerator AssignPathAnimationCoroutine(Property property, float duration, float startTime, Functions easing)
        {
            PointDefinitionInterpolation pointDataInterpolation = property.Value as PointDefinitionInterpolation;
            while (true)
            {
                float elapsedTime = Instance.CustomEventCallbackController.AudioTimeSource.songTime - startTime;
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
