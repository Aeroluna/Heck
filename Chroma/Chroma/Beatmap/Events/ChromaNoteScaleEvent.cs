using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using CustomJSONData.CustomBeatmap;
using CustomJSONData;
using Chroma.Utils;

namespace Chroma.Beatmap.Events {

    public class ChromaNoteScaleEvent {

        public static Dictionary<float, float> NoteScales = new Dictionary<float, float>();

        // Creates dictionary loaded with all _noteScale custom events and indexs them with the event's time
        public static void Activate(List<CustomEventData> eventData) {
            if (!ChromaUtils.CheckSpecialEventRequirement()) return;
            foreach (CustomEventData d in eventData) {
                try {
                    dynamic dynData = d.data;
                    float s = (float)Trees.at(dynData, "_scale");
                    NoteScales.Add(d.time, s);
                    //ChromaLogger.Log("Global note scale registered: " + s.ToString());
                }
                catch (Exception e) {
                    ChromaLogger.Log("INVALID CUSTOM EVENT", ChromaLogger.Level.WARNING);
                    ChromaLogger.Log(e);
                }
            }
            ChromaLogger.Log("CREATED:" + NoteScales.Count);
        }
    }

}
