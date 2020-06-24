using Chroma.Utils;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Events
{
    internal class ChromaObstacleColourEvent
    {
        internal static List<TimedColor> ObstacleColours = new List<TimedColor>();

        // Creates dictionary loaded with all _obstacleColor custom events and indexs them with the event's time
        internal static void Activate(List<CustomEventData> eventData)
        {
            foreach (CustomEventData d in eventData)
            {
                try
                {
                    dynamic dynData = d.data;
                    Color c = ChromaUtils.GetColorFromData(dynData);
                    ObstacleColours.Add(new TimedColor(d.time, c));

                    ColourManager.TechnicolourBarriersForceDisabled = true;
                }
                catch (Exception e)
                {
                    Logger.Log("INVALID CUSTOM EVENT", Logger.Level.WARNING);
                    Logger.Log(e);
                }
            }
        }
    }
}