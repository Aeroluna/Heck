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
    internal static class DissolveArrow
    {
        private static Dictionary<Track, Coroutine> _activeCoroutines = new Dictionary<Track, Coroutine>();

        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "DissolveArrow")
            {
                Track track = GetTrack(customEventData.data);
                if (track != null)
                {
                    float start = (float?)Trees.at(customEventData.data, START) ?? 1f;
                    float end = (float?)Trees.at(customEventData.data, END) ?? 0f;
                    float duration = (float?)Trees.at(customEventData.data, DURATION) ?? 1.4f;
                    string easingString = Trees.at(customEventData.data, EASING);
                    Easings.Functions easing = Easings.InterprateString(easingString);

                    if (_activeCoroutines.TryGetValue(track, out Coroutine coroutine) && coroutine != null) _instance.StopCoroutine(coroutine);
                    _activeCoroutines[track] = _instance.StartCoroutine(DissolveArrowCoroutine(start, end, duration, customEventData.time, easing, track));
                }
            }
        }

        private static IEnumerator DissolveArrowCoroutine(float cutoutStart, float cutoutEnd, float duration, float startTime, Easings.Functions easing, Track track)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime = _customEventCallbackController._audioTimeSource.songTime - startTime;
                float time = elapsedTime / duration;
                float cutout = Mathf.Lerp(cutoutStart, cutoutEnd, Easings.Interpolate(time, easing));
                track.dissolveArrow = cutout;
                yield return null;
            }
            track.dissolveArrow = cutoutEnd;
            _activeCoroutines.Remove(track);
            yield break;
        }
    }
}