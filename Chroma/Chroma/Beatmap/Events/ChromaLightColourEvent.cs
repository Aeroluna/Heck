using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomJSONData.CustomBeatmap;
using CustomJSONData;
using UnityEngine;
using Chroma.Utils;

namespace Chroma.Beatmap.Events {

    class ChromaLightColourEvent {

        public static Dictionary<BeatmapEventType, Dictionary<float, Color>> CustomLightColours = new Dictionary<BeatmapEventType, Dictionary<float, Color>>();

        // Creates dictionary loaded with all _lightRGB custom events and indexs them with the event's time and type
        public static void Activate(List<CustomEventData> eventData) {
            if (!ChromaUtils.CheckLightingEventRequirement()) return;
            foreach (CustomEventData d in eventData) {
                try {
                    dynamic dynData = d.data;
                    int id = (int)Trees.at(dynData, "_lightsID");
                    float r = (float)Trees.at(dynData, "r");
                    float g = (float)Trees.at(dynData, "g");
                    float b = (float)Trees.at(dynData, "b");
                    float? a = (float?)Trees.at(dynData, "a");
                    Color c = new Color(r, g, b);
                    if (a != null) c = c.ColorWithAlpha((float)a);

                    // Dictionary of dictionaries!
                    Dictionary<float, Color> dictionaryID;
                    if (!CustomLightColours.TryGetValue((BeatmapEventType)id, out dictionaryID)) {
                        dictionaryID = new Dictionary<float, Color>();
                        CustomLightColours.Add((BeatmapEventType)id, dictionaryID);
                    }
                    dictionaryID.Add(d.time, c);
                    //ChromaLogger.Log("Global light colour registered: " + c.ToString());
                }
                catch (Exception e) {
                    ChromaLogger.Log("INVALID CUSTOM EVENT", ChromaLogger.Level.WARNING);
                    ChromaLogger.Log(e);
                }
            }
        }
    }
}
