using Chroma.Extensions;
using Chroma.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Events
{
    internal class ChromaGradientEvent : MonoBehaviour
    {
        internal static Dictionary<BeatmapEventType, ChromaGradientEvent> Gradients = new Dictionary<BeatmapEventType, ChromaGradientEvent>();

        private static GameObject Instance
        {
            get
            {
                if (_instance == null) _instance = new GameObject("Chroma_GradientController");
                return _instance;
            }
        }

        private static GameObject _instance;

        internal static void Clear()
        {
            if (_instance != null) Destroy(_instance);
            _instance = null;
        }

        private static ChromaGradientEvent Instantiate(Color initc, Color endc, float start, float dur, BeatmapEventType type, Easings.Functions easing)
        {
            ChromaGradientEvent gradient = Instance.AddComponent<ChromaGradientEvent>();
            gradient._initcolor = initc;
            gradient._endcolor = endc;
            gradient._start = start;
            gradient._duration = (60f * dur) / ChromaBehaviour.songBPM;
            gradient._event = type;
            gradient._easing = easing;
            return gradient;
        }

        private void Update()
        {
            float _time = ChromaBehaviour.ATSC.songTime - _start;
            if (_time > 0 && _time <= _duration)
            {
                Color c = Color.Lerp(_initcolor, _endcolor, Easings.Interpolate(_time / _duration, _easing));
                _event.SetLightingColours(c, c);
                _event.SetActiveColours();
            }
            else
            {
                if (_time > _duration)
                {
                    _event.SetLightingColours(_endcolor, _endcolor);
                    _event.SetActiveColours();
                }
                Gradients.Remove(_event);
                Destroy(this);
            }
        }

        private Color _initcolor;
        private Color _endcolor;
        private float _start;
        private float _duration;
        private Easings.Functions _easing;
        private BeatmapEventType _event;

        internal static Color AddGradient(BeatmapEventType id, Color initc, Color endc, float time, float duration, Easings.Functions easing)
        {
            float _time = ChromaBehaviour.ATSC.songTime - time;
            if (_time < duration)
            {
                if (Gradients.TryGetValue(id, out ChromaGradientEvent gradient))
                {
                    Destroy(gradient);
                    Gradients.Remove(id);
                }
                Gradients.Add(id, Instantiate(initc, endc, time, duration, id, easing));
                return Color.Lerp(initc, endc, _time / duration); ;
            }
            else return endc;
        }
    }
}