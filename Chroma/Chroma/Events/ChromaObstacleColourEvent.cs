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
            if (!ChromaBehaviour.LightingRegistered) return;
            foreach (CustomEventData d in eventData)
            {
                try
                {
                    dynamic dynData = d.data;
                    float r = (float)Trees.at(dynData, "_r");
                    float g = (float)Trees.at(dynData, "_g");
                    float b = (float)Trees.at(dynData, "_b");
                    float? a = (float?)Trees.at(dynData, "_a");
                    Color c = new Color(r, g, b);
                    if (a.HasValue) c = c.ColorWithAlpha(a.Value);
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