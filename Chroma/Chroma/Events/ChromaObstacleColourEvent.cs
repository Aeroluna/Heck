using Chroma.Utils;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Events
{
    internal class ChromaObstacleColourEvent
    {
        internal static Dictionary<float, Color> CustomObstacleColours = new Dictionary<float, Color>();

        // Creates dictionary loaded with all _obstacleColor custom events and indexs them with the event's time
        internal static void Activate(List<CustomEventData> eventData)
        {
            foreach (CustomEventData d in eventData)
            {
                try
                {
                    dynamic dynData = d.data;
                    Color c = ChromaUtils.GetColorFromData(dynData);
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