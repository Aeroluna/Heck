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
    // TODO: like all this stuff
    internal class AnimateVisualTrack
    {
        private static Dictionary<Track, Coroutine> _activeCoroutines = new Dictionary<Track, Coroutine>();

        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "AnimateVisualTrack")
            {
                Track track = GetTrack(customEventData.data);
                if (track != null)
                {
                    float duration = (float)Trees.at(customEventData.data, DURATION);

                    GetPointData(customEventData.data, out PointData position, out PointData rotation, out PointData scale, out PointData localRotation);

                    if (_activeCoroutines.TryGetValue(track, out Coroutine coroutine) && coroutine != null) _instance.StopCoroutine(coroutine);
                    _activeCoroutines[track] = _instance.StartCoroutine(AnimateVisualTrackCoroutine(position, rotation, scale, localRotation, duration, customEventData.time, track));
                }
            }
        }

        private static IEnumerator AnimateVisualTrackCoroutine(PointData position, PointData rotation, PointData scale, PointData localRotation,
            float duration, float startTime, Track track)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime = _customEventCallbackController._audioTimeSource.songTime - startTime;
                float time = elapsedTime / duration;
                if (position != null) track.visualPosition = position.Interpolate(time);
                if (rotation != null) track.visualRotation = rotation.Interpolate(time);
                if (scale != null) track.visualScale = scale.Interpolate(time);
                if (localRotation != null) track.visualLocalRotation = localRotation.Interpolate(time);
                yield return null;
            }
            if (position != null) track.visualPosition = position.Interpolate(1);
            if (rotation != null) track.visualRotation = rotation.Interpolate(1);
            if (scale != null) track.visualScale = scale.Interpolate(1);
            if (localRotation != null) track.visualLocalRotation = localRotation.Interpolate(1);
            _activeCoroutines.Remove(track);
            yield break;
        }
    }
}