namespace Chroma
{
    using System;
    using System.Collections.Generic;
    using Chroma.Extensions;
    using Chroma.Utils;
    using CustomJSONData;
    using UnityEngine;

    internal class ChromaGradientController : MonoBehaviour
    {
        private static ChromaGradientController _instance;

        private static ChromaGradientController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("Chroma_GradientController").AddComponent<ChromaGradientController>();
                }

                return _instance;
            }
        }

        private IDictionary<BeatmapEventType, ChromaGradientEvent> Gradients { get; } = new Dictionary<BeatmapEventType, ChromaGradientEvent>();

        internal static bool IsGradientActive(BeatmapEventType eventType)
        {
            return Instance.Gradients.ContainsKey(eventType);
        }

        internal static void CancelGradient(BeatmapEventType eventType)
        {
            Instance.Gradients.Remove(eventType);
        }

        internal static void Clear()
        {
            if (_instance != null)
            {
                _instance.Gradients.Clear();
                Destroy(_instance.gameObject);
            }

            _instance = null;
        }

        internal static Color AddGradient(dynamic gradientObject, BeatmapEventType id, float time)
        {
            float duration = (float)Trees.at(gradientObject, "_duration");
            Color initcolor = ChromaUtils.GetColorFromData(gradientObject, "_startColor");
            Color endcolor = ChromaUtils.GetColorFromData(gradientObject, "_endColor");
            string easingstring = (string)Trees.at(gradientObject, "_easing");
            Functions easing;
            if (string.IsNullOrEmpty(easingstring))
            {
                easing = Functions.easeLinear;
            }
            else
            {
                easing = (Functions)Enum.Parse(typeof(Functions), easingstring);
            }

            ChromaGradientEvent gradientEvent = new ChromaGradientEvent(initcolor, endcolor, time, duration, id, easing);
            Instance.Gradients[id] = gradientEvent;
            return gradientEvent.Interpolate();
        }

        private void Update()
        {
            foreach (KeyValuePair<BeatmapEventType, ChromaGradientEvent> valuePair in new Dictionary<BeatmapEventType, ChromaGradientEvent>(Gradients))
            {
                Color color = valuePair.Value.Interpolate();
                BeatmapEventType eventType = valuePair.Key;
                eventType.SetLightingColors(color, color);
                eventType.SetActiveColors();
            }
        }

        private struct ChromaGradientEvent
        {
            internal readonly Color _initcolor;
            internal readonly Color _endcolor;
            internal readonly float _start;
            internal readonly float _duration;
            internal readonly BeatmapEventType _event;
            internal readonly Functions _easing;

            internal ChromaGradientEvent(Color initcolor, Color endcolor, float start, float duration, BeatmapEventType eventType, Functions easing = Functions.easeLinear)
            {
                _initcolor = initcolor;
                _endcolor = endcolor;
                _start = start;
                _duration = 60f * duration / ChromaController.SongBPM;
                _event = eventType;
                _easing = easing;
            }

            internal Color Interpolate()
            {
                float normalTime = ChromaController.AudioTimeSyncController.songTime - _start;
                if (normalTime < 0)
                {
                    return _initcolor;
                }
                else if (normalTime <= _duration)
                {
                    return Color.LerpUnclamped(_initcolor, _endcolor, Easings.Interpolate(normalTime / _duration, _easing));
                }
                else
                {
                    Instance.Gradients.Remove(_event);
                    return _endcolor;
                }
            }
        }
    }
}
