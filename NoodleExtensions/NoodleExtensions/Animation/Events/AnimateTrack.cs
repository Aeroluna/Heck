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
                    float duration = (float)Trees.at(customEventData.data, DURATION);

                    GetPointData(customEventData.data, out PointData position, out PointData rotation, out PointData scale, out PointData localRotation);

                    if (_activeCoroutines.TryGetValue(track, out Coroutine coroutine) && coroutine != null) _instance.StopCoroutine(coroutine);
                    _activeCoroutines[track] = _instance.StartCoroutine(AnimateTrackCoroutine(position, rotation, scale, localRotation, duration, customEventData.time, track));
                }
            }
        }

        private static IEnumerator AnimateTrackCoroutine(PointData position, PointData rotation, PointData scale, PointData localRotation,
            float duration, float startTime, Track track)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime = _customEventCallbackController._audioTimeSource.songTime - startTime;
                float time = elapsedTime / duration;
                if (position != null) track.position = position.Interpolate(time);
                if (rotation != null) track.rotation = rotation.Interpolate(time);
                if (scale != null) track.scale = scale.Interpolate(time);
                if (localRotation != null) track.localRotation = localRotation.Interpolate(time);
                yield return null;
            }
            if (position != null) track.position = position.Interpolate(1);
            if (rotation != null) track.rotation = rotation.Interpolate(1);
            if (scale != null) track.scale = scale.Interpolate(1);
            if (localRotation != null) track.localRotation = localRotation.Interpolate(1);
            _activeCoroutines.Remove(track);
            yield break;
        }
    }
}