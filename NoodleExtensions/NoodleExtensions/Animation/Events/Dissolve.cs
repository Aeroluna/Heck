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
    internal static class Dissolve
    {
        private static readonly FieldAccessor<BaseNoteVisuals, CutoutAnimateEffect>.Accessor _cutoutAnimateEffectAccessor = FieldAccessor<BaseNoteVisuals, CutoutAnimateEffect>.GetAccessor("_cutoutAnimateEffect");
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "Dissolve")
            {
                Track track = GetTrack(customEventData);
                if (track != null)
                {
                    float start = (float?)Trees.at(customEventData.data, "_start") ?? 0f;
                    float end = (float?)Trees.at(customEventData.data, "_end") ?? 1f;
                    float duration = (float?)Trees.at(customEventData.data, "_duration") ?? 1.4f;
                    string easingString = Trees.at(customEventData.data, "_easing");
                    Easings.Functions easing = string.IsNullOrEmpty(easingString) ? Easings.Functions.easeLinear : (Easings.Functions)Enum.Parse(typeof(Easings.Functions), easingString);

                    foreach (NoteController noteController in GetActiveBasicNotes(track))
                    {
                        BaseNoteVisuals baseNoteVisuals = noteController.gameObject.GetComponent<BaseNoteVisuals>();
                        CutoutAnimateEffect cutoutAnimateEffect = _cutoutAnimateEffectAccessor(ref baseNoteVisuals);
                        _instance.StartCoroutine(DissolveCoroutine(start, end, duration, customEventData.time, cutoutAnimateEffect, easing));
                    }
                }
            }
        }

        private static IEnumerator DissolveCoroutine(float cutoutStart, float cutoutEnd, float duration, float startTime, CutoutAnimateEffect cutoutAnimateEffect, Easings.Functions easing)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime = _customEventCallbackController._audioTimeSource.songTime - startTime;
                float time = elapsedTime / duration;
                cutoutAnimateEffect.SetCutout(1 - Mathf.Lerp(cutoutStart, cutoutEnd, Easings.Interpolate(time, easing)));
                yield return null;
            }
            cutoutAnimateEffect.SetCutout(1 - cutoutEnd);
            yield break;
        }
    }
}
