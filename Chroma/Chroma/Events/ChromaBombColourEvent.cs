using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Events
{
    internal class ChromaBombColourEvent
    {
        internal static Dictionary<float, Color> CustomBombColours = new Dictionary<float, Color>();

        // Creates dictionary loaded with all _bombColor custom events and indexs them with the event's time
        internal static void Activate(List<CustomEventData> eventData)
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
                    CustomBombColours.Add(d.time, c);

                    ColourManager.TechnicolourBombsForceDisabled = true;
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