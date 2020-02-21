using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Events
{
    internal class ChromaGradientEvent : MonoBehaviour
    {
        public static Dictionary<BeatmapEventType, ChromaGradientEvent> CustomGradients = new Dictionary<BeatmapEventType, ChromaGradientEvent>();

        public static GameObject Instance
        {
            get
            {
                if (_instance == null) _instance = new GameObject("Chroma_GradientController");
                return _instance;
            }
        }

        private static GameObject _instance;

        public static void Clear()
        {
            if (_instance != null) Destroy(_instance);
            _instance = null;
        }

        public static ChromaGradientEvent Instantiate(Color initc, Color endc, float start, float dur, BeatmapEventType type)
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
                ColourManager.RecolourLight(_event, c, c);
                if (ColourManager.LightSwitchs[_event].GetPrivateField<bool>("_lightIsOn"))
                    ColourManager.LightSwitchs[_event].SetColor(c);
            }
            else
            {
                if (_time > _duration) ColourManager.RecolourLight(_event, _endcolor, _endcolor);
                CustomGradients.Remove(_event);
                Destroy(this);
            }
        }

        public Color _initcolor;
        public Color _endcolor;
        public float _start;
        public float _duration;
        public BeatmapEventType _event;

        public static Color AddGradient(BeatmapEventType id, Color initc, Color endc, float time, float duration)
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

        // Creates dictionary loaded with all _lightGradient custom events and indexs them with the event's time and type
        public static void Callback(CustomEventData eventData)
        {
            try
            {
                // Pull and assign all custom data
                dynamic dynData = eventData.data;
                int intid = (int)Trees.at(dynData, "_event");
                float duration = (float)Trees.at(dynData, "_duration");
                List<object> initcolor = Trees.at(dynData, "_startColor");
                List<object> endcolor = Trees.at(dynData, "_endColor");

                float initr = Convert.ToSingle(initcolor[0]);
                float initg = Convert.ToSingle(initcolor[1]);
                float initb = Convert.ToSingle(initcolor[2]);
                float endr = Convert.ToSingle(endcolor[0]);
                float endg = Convert.ToSingle(endcolor[1]);
                float endb = Convert.ToSingle(endcolor[2]);

                BeatmapEventType id = (BeatmapEventType)intid;
                Color initc = new Color(initr, initg, initb);
                Color endc = new Color(endr, endg, endb);
                if (initcolor.Count > 3) initc = initc.ColorWithAlpha(Convert.ToSingle(initcolor[3]));
                if (endcolor.Count > 3) endc = endc.ColorWithAlpha(Convert.ToSingle(endcolor[3]));

                AddGradient(id, initc, endc, eventData.time, duration);
            }
            catch (Exception e)
            {
                ChromaLogger.Log("INVALID CUSTOM EVENT", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }
        }
    }
}