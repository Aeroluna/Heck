using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPA.Utilities;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using System.Threading.Tasks;
using static NoodleExtensions.Animation.AnimationController;
using UnityEngine;
using System.Collections;

namespace NoodleExtensions.Animation
{
    internal static class DissolveArrow
    {
        private static Dictionary<Track, Coroutine> _activeCoroutines = new Dictionary<Track, Coroutine>();
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "DissolveArrow")
            {
                Track track = GetTrack(customEventData);
                if (track != null)
                {
                    float start = (float?)Trees.at(customEventData.data, "_start") ?? 1f;
                    float end = (float?)Trees.at(customEventData.data, "_end") ?? 0f;
                    float duration = (float?)Trees.at(customEventData.data, "_duration") ?? 1.4f;
                    string easingString = Trees.at(customEventData.data, "_easing");
                    Easings.Functions easing = string.IsNullOrEmpty(easingString) ? Easings.Functions.easeLinear : (Easings.Functions)Enum.Parse(typeof(Easings.Functions), easingString);

                    List<DisappearingArrowController> disappearingArrowControllers = new List<DisappearingArrowController>();
                    foreach (NoteController noteController in GetActiveBasicNotes(track))
                    {
                        DisappearingArrowController disappearingArrowController = noteController.gameObject.GetComponent<DisappearingArrowController>();
                        disappearingArrowControllers.Add(disappearingArrowController);
                    }

                    if (_activeCoroutines.TryGetValue(track, out Coroutine coroutine) && coroutine != null) _instance.StopCoroutine(coroutine);
                    _activeCoroutines[track] = _instance.StartCoroutine(DissolveArrowCoroutine(start, end, duration, customEventData.time, disappearingArrowControllers, easing, track));
                }
            }
        }

        private static IEnumerator DissolveArrowCoroutine(float cutoutStart, float cutoutEnd, float duration, float startTime,
            List<DisappearingArrowController> disappearingArrowControllers, Easings.Functions easing, Track track)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime = _customEventCallbackController._audioTimeSource.songTime - startTime;
                float time = elapsedTime / duration;
                float cutout = Mathf.Lerp(cutoutStart, cutoutEnd, Easings.Interpolate(time, easing));
                disappearingArrowControllers.ForEach(n => n.SetArrowTransparency(cutout));
                yield return null;
            }
            disappearingArrowControllers.ForEach(n => n.SetArrowTransparency(cutoutEnd));
            _activeCoroutines.Remove(track);
            yield break;
        }
    }
}
