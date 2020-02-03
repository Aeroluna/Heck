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

    class ChromaObstacleColorEvent {

        public static Dictionary<float, Color> CustomObstacleColors = new Dictionary<float, Color>();
        
        // Creates dictionary loaded with all _obstacleColor custom events and indexs them with the event's time
        public static void Activate(List<CustomEventData> eventData) {
            CustomObstacleColors.Clear();
            if (!ChromaUtils.CheckSpecialEventRequirement()) return;
            foreach (CustomEventData d in eventData) {
                try {
                    dynamic dynData = d.data;
                    float r = (float)Trees.at(dynData, "r");
                    float g = (float)Trees.at(dynData, "g");
                    float b = (float)Trees.at(dynData, "b");
                    Color c = new Color(r, g, b);
                    CustomObstacleColors.Add(d.time, c);
                    //ChromaLogger.Log("Global barrier colour registered: " + c.ToString());
                }
                catch (Exception e) {
                    ChromaLogger.Log("INVALID CUSTOM EVENT", ChromaLogger.Level.WARNING);
                    ChromaLogger.Log(e);
                }
            }
        }
    }
}
