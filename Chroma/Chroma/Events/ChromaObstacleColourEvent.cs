using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Events
{
    internal class ChromaObstacleColourEvent
    {
        public static Dictionary<float, Color> CustomObstacleColours = new Dictionary<float, Color>();

        // Creates dictionary loaded with all _obstacleColor custom events and indexs them with the event's time
        public static void Activate(List<CustomEventData> eventData)
        {
            foreach (CustomEventData d in eventData)
            {
                try
                {
                    dynamic dynData = d.data;
                    List<object> color = Trees.at(dynData, "_color");
                    float r = Convert.ToSingle(color[0]);
                    float g = Convert.ToSingle(color[1]);
                    float b = Convert.ToSingle(color[2]);

                    Color c = new Color(r, g, b);
                    if (color.Count > 3) c = c.ColorWithAlpha(Convert.ToSingle(color[3]));
                    CustomObstacleColours.Add(d.time, c);

                    ColourManager.TechnicolourBarriersForceDisabled = true;
                }
                catch (Exception e)
                {
                    ChromaLogger.Log("INVALID CUSTOM EVENT", ChromaLogger.Level.WARNING);
                    ChromaLogger.Log(e);
                }
            }
        }
    }
}