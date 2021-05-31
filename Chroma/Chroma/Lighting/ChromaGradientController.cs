namespace Chroma
{
    using System.Collections.Generic;
    using Chroma.Colorizer;
    using Chroma.Utils;
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

        internal static Color AddGradient(ChromaLightEventData.GradientObjectData gradientObject, BeatmapEventType id, float time)
        {
            CancelGradient(id);

            float duration = gradientObject.Duration;
            Color initcolor = gradientObject.StartColor;
            Color endcolor = gradientObject.EndColor;
            Functions easing = gradientObject.Easing;

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
                eventType.ColorizeLight(color, color, color, color);
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
                _duration = 60f * duration / ChromaController.BeatmapObjectSpawnController.currentBpm;
                _event = eventType;
                _easing = easing;
            }

            internal Color Interpolate()
            {
                float normalTime = ChromaController.IAudioTimeSource.songTime - _start;
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
