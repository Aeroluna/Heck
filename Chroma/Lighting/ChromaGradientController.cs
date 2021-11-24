using System.Collections.Generic;
using Chroma.Colorizer;
using Heck.Animation;
using IPA.Utilities;
using UnityEngine;

namespace Chroma.Lighting
{
    internal class ChromaGradientController : MonoBehaviour
    {
        private static ChromaGradientController? _instance;

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

        internal static Color AddGradient(ChromaEventData.GradientObjectData gradientObject, BeatmapEventType id, float time)
        {
            CancelGradient(id);

            float duration = gradientObject.Duration;
            Color initcolor = gradientObject.StartColor;
            Color endcolor = gradientObject.EndColor;
            Functions easing = gradientObject.Easing;

            ChromaGradientEvent gradientEvent = new(initcolor, endcolor, time, duration, id, easing);
            Instance.Gradients[id] = gradientEvent;
            return gradientEvent.Interpolate();
        }

        private void Update()
        {
            foreach ((BeatmapEventType eventType, ChromaGradientEvent value) in new Dictionary<BeatmapEventType, ChromaGradientEvent>(Gradients))
            {
                Color color = value.Interpolate();
                eventType.ColorizeLight(true, color, color, color, color);
            }
        }

        private readonly struct ChromaGradientEvent
        {
            private readonly Color _initcolor;
            private readonly Color _endcolor;
            private readonly float _start;
            private readonly float _duration;
            private readonly BeatmapEventType _event;
            private readonly Functions _easing;

            internal ChromaGradientEvent(Color initcolor, Color endcolor, float start, float duration, BeatmapEventType eventType, Functions easing = Functions.easeLinear)
            {
                _initcolor = initcolor;
                _endcolor = endcolor;
                _start = start;
                _duration = 60f * duration / ChromaController.BeatmapObjectSpawnController!.currentBpm;
                _event = eventType;
                _easing = easing;
            }

            internal Color Interpolate()
            {
                float normalTime = ChromaController.IAudioTimeSource!.songTime - _start;
                if (normalTime < 0)
                {
                    return _initcolor;
                }

                if (normalTime <= _duration)
                {
                    return Color.LerpUnclamped(_initcolor, _endcolor, Easings.Interpolate(normalTime / _duration, _easing));
                }

                Instance.Gradients.Remove(_event);
                return _endcolor;
            }
        }
    }
}
