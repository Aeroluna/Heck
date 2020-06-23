using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NoodleExtensions.Animation.AnimationController;
using static NoodleExtensions.Animation.AnimationHelper;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.Animation
{
    internal class AssignPathAnimation
    {
        private static Dictionary<Track, Coroutine> _activeCoroutines = new Dictionary<Track, Coroutine>();

        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "AssignPathAnimation")
            {
                Track track = GetTrack(customEventData.data);
                if (track != null)
                {
                    float duration = (float?)Trees.at(customEventData.data, DURATION) ?? 0f;
                    duration = (60f * duration) / instance.beatmapObjectSpawnController.currentBPM; // Convert to real time

                    string easingString = (string)Trees.at(customEventData.data, EASING);
                    Functions easing = Functions.easeLinear;
                    if (easingString != null) easing = (Functions)Enum.Parse(typeof(Functions), easingString);

                    GetAllPointData(customEventData.data, out PointData position, out PointData rotation, out PointData scale, out PointData localRotation, out PointData dissolve, out PointData dissolveArrow);
                    GetDefinitePosition(customEventData.data, out PointData definitePosition);

                    if (position != null) track._pathPosition.Init(position);
                    if (rotation != null) track._pathRotation.Init(rotation);
                    if (scale != null) track._pathScale.Init(scale);
                    if (localRotation != null) track._pathLocalRotation.Init(localRotation);
                    if (definitePosition != null) track._pathDefinitePosition.Init(definitePosition);
                    if (dissolve != null) track._pathDissolve.Init(dissolve);
                    if (dissolveArrow != null) track._pathDissolveArrow.Init(dissolveArrow);

                    if (_activeCoroutines.TryGetValue(track, out Coroutine coroutine) && coroutine != null) instance.StopCoroutine(coroutine);
                    _activeCoroutines[track] = instance.StartCoroutine(AssignPathAnimationCoroutine(position, rotation, scale, localRotation, definitePosition, dissolve, dissolveArrow, duration, customEventData.time, track, easing));
                }
            }
        }

        private static IEnumerator AssignPathAnimationCoroutine(PointData position, PointData rotation, PointData scale, PointData localRotation, PointData definitePosition, PointData dissolve, PointData dissolveArrow,
            float duration, float startTime, Track track, Functions easing)
        {
            while(true)
            {
                float elapsedTime = instance.customEventCallbackController._audioTimeSource.songTime - startTime;
                track._pathInterpolationTime = Easings.Interpolate(Mathf.Min(elapsedTime / duration, 1f), easing);

                if (elapsedTime < duration) yield return null;
                else break;
            }

            if (position != null) track._pathPosition.Finish();
            if (rotation != null) track._pathRotation.Finish();
            if (scale != null) track._pathScale.Finish();
            if (localRotation != null) track._pathLocalRotation.Finish();
            if (definitePosition != null) track._pathDefinitePosition.Finish();
            if (dissolve != null) track._pathDissolve.Finish();
            if (dissolveArrow != null) track._pathDissolveArrow.Finish();
        }
    }
}
