using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NoodleExtensions.Animation.AnimationController;
using static NoodleExtensions.Animation.AnimationHelper;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.Animation
{
    internal class AnimateTrack
    {
        private static Dictionary<Track, Coroutine> _activeCoroutines = new Dictionary<Track, Coroutine>();

        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "AnimateTrack")
            {
                Track track = GetTrack(customEventData.data);
                if (track != null)
                {
                    float duration = (float?)Trees.at(customEventData.data, DURATION) ?? 0f;
                    duration = (60f * duration) / instance.beatmapObjectSpawnController.currentBPM; // Convert to real time;

                    GetAllPointData(customEventData.data, out PointData position, out PointData rotation, out PointData scale, out PointData localRotation, out PointData dissolve, out PointData dissolveArrow);

                    if (_activeCoroutines.TryGetValue(track, out Coroutine coroutine) && coroutine != null) instance.StopCoroutine(coroutine);
                    _activeCoroutines[track] = instance.StartCoroutine(AnimateTrackCoroutine(position, rotation, scale, localRotation, dissolve, dissolveArrow, duration, customEventData.time, track));
                }
            }
        }

        private static IEnumerator AnimateTrackCoroutine(PointData position, PointData rotation, PointData scale, PointData localRotation, PointData dissolve, PointData dissolveArrow,
            float duration, float startTime, Track track)
        {
            float elapsedTime = -1;
            while (elapsedTime < duration)
            {
                elapsedTime = instance.customEventCallbackController._audioTimeSource.songTime - startTime;
                float time = Mathf.Min(elapsedTime / duration, 1f);
                if (position != null) track._position = position.Interpolate(time);
                if (rotation != null) track._rotation = rotation.InterpolateQuaternion(time);
                if (scale != null) track._scale = scale.Interpolate(time);
                if (localRotation != null) track._localRotation = localRotation.InterpolateQuaternion(time);
                if (dissolve != null) track._dissolve = dissolve.InterpolateLinear(time);
                if (dissolveArrow != null) track._dissolveArrow = dissolveArrow.InterpolateLinear(time);
                yield return null;
            }
            _activeCoroutines.Remove(track);
            yield break;
        }
    }
}