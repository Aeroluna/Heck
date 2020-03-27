using Chroma.Extensions;
using Chroma.Utils;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using BS_Utils.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Events
{
    internal class ChromaGradientEvent : MonoBehaviour
    {
        internal static Dictionary<BeatmapEventType, ChromaGradientEvent> CustomGradients = new Dictionary<BeatmapEventType, ChromaGradientEvent>();

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

        private static ChromaGradientEvent Instantiate(Color initc, Color endc, float start, float dur, BeatmapEventType type)
        {
            ChromaGradientEvent gradient = Instance.AddComponent<ChromaGradientEvent>();
            gradient._initcolor = initc;
            gradient._endcolor = endc;
            gradient._start = start;
            gradient._duration = (60 * dur) / ChromaBehaviour.songBPM;
            gradient._event = type;
            return gradient;
        }

        private void Update()
        {
            float _time = ChromaBehaviour.ATSC.songTime - _start;
            if (_time < 0 || _time <= _duration)
            {
                Color c = Color.Lerp(_initcolor, _endcolor, _time / _duration);
                _event.SetLightingColours(c, c);
                if (ColourManager.LightSwitchDictionary[_event].GetPrivateField<bool>("_lightIsOn"))
                    ColourManager.LightSwitchDictionary[_event].SetColor(c);
            }
            else
            {
                if (_time > _duration) _event.SetLightingColours(_endcolor, _endcolor);
                CustomGradients.Remove(_event);
                Destroy(this);
            }
        }

        private Color _initcolor;
        private Color _endcolor;
        private float _start;
        private float _duration;
        private BeatmapEventType _event;

        internal static Color AddGradient(BeatmapEventType id, Color initc, Color endc, float time, float duration)
        {
            if (ChromaBehaviour.ATSC.songTime - time < duration)
            {
                if (CustomGradients.TryGetValue(id, out ChromaGradientEvent gradient))
                {
                    Destroy(gradient);
                    CustomGradients.Remove(id);
                }
                CustomGradients.Add(id, Instantiate(initc, endc, time, duration, id));
                return initc;
            }
            else return endc;
        }
    }
}