namespace Chroma.Events
{
    using System.Collections.Generic;
    using Chroma.Extensions;
    using Chroma.Utils;
    using UnityEngine;

    internal class ChromaGradientEvent : MonoBehaviour
    {
        private static GameObject _instance;

        private Color _initcolor;
        private Color _endcolor;
        private float _start;
        private float _duration;
        private Functions _easing;
        private BeatmapEventType _event;

        internal static Dictionary<BeatmapEventType, ChromaGradientEvent> Gradients { get; } = new Dictionary<BeatmapEventType, ChromaGradientEvent>();

        private static GameObject Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("Chroma_GradientController");
                }

                return _instance;
            }
        }

        internal static void Clear()
        {
            if (_instance != null)
            {
                Destroy(_instance);
            }

            _instance = null;
        }

        internal static Color AddGradient(BeatmapEventType id, Color initc, Color endc, float time, float duration, Functions easing)
        {
            float normalTime = ChromaBehaviour.ATSC.songTime - time;
            if (normalTime < duration)
            {
                if (Gradients.TryGetValue(id, out ChromaGradientEvent gradient))
                {
                    Destroy(gradient);
                    Gradients.Remove(id);
                }

                Gradients.Add(id, Instantiate(initc, endc, time, duration, id, easing));
                return Color.Lerp(initc, endc, normalTime / duration);
            }
            else
            {
                return endc;
            }
        }

        private static ChromaGradientEvent Instantiate(Color initc, Color endc, float start, float dur, BeatmapEventType type, Functions easing)
        {
            ChromaGradientEvent gradient = Instance.AddComponent<ChromaGradientEvent>();
            gradient._initcolor = initc;
            gradient._endcolor = endc;
            gradient._start = start;
            gradient._duration = (60f * dur) / ChromaBehaviour.SongBPM;
            gradient._event = type;
            gradient._easing = easing;
            return gradient;
        }

        private void Update()
        {
            float time = ChromaBehaviour.ATSC.songTime - _start;
            if (time > 0 && time <= _duration)
            {
                Color c = Color.Lerp(_initcolor, _endcolor, Easings.Interpolate(time / _duration, _easing));
                _event.SetLightingColors(c, c);
                _event.SetActiveColors();
            }
            else
            {
                if (time > _duration)
                {
                    _event.SetLightingColors(_endcolor, _endcolor);
                    _event.SetActiveColors();
                }

                Gradients.Remove(_event);
                Destroy(this);
            }
        }
    }
}
