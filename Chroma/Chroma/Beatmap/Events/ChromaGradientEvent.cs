using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Beatmap.Events
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
            if (_time <= _duration)
            {
                Color c = Color.Lerp(_initcolor, _endcolor, _time / _duration);
                ColourManager.RecolourLight(_event, c, c);
                if (ColourManager.LightSwitchs[_event].GetPrivateField<bool>("_lightIsOn"))
                    ColourManager.LightSwitchs[_event].SetColor(c);
            }
            else
            {
                CustomGradients.Remove(_event);
                Destroy(this);
            }
        }

        public Color _initcolor;
        public Color _endcolor;
        public float _start;
        public float _duration;
        public BeatmapEventType _event;

        public static void AddGradient(BeatmapEventType id, Color initc, Color endc, float time, float duration)
        {
            if (CustomGradients.TryGetValue(id, out ChromaGradientEvent gradient))
            {
                Destroy(gradient);
                CustomGradients.Remove(id);
            }
            CustomGradients.Add(id, Instantiate(initc, endc, time, duration, id));
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
                float initr = (float)Trees.at(dynData, "_startR");
                float initg = (float)Trees.at(dynData, "_startG");
                float initb = (float)Trees.at(dynData, "_startB");
                float? inita = (float?)Trees.at(dynData, "_startA");
                float endr = (float)Trees.at(dynData, "_endR");
                float endg = (float)Trees.at(dynData, "_endG");
                float endb = (float)Trees.at(dynData, "_endB");
                float? enda = (float?)Trees.at(dynData, "_endA");

                BeatmapEventType id = (BeatmapEventType)intid;
                Color initc = new Color(initr, initg, initb);
                Color endc = new Color(endr, endg, endb);
                if (inita.HasValue) initc = initc.ColorWithAlpha(inita.Value);
                if (enda.HasValue) endc = endc.ColorWithAlpha(enda.Value);

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