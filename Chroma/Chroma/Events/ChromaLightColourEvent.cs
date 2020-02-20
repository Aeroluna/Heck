using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Events
{
    internal class ChromaLightColourEvent
    {
        public static Dictionary<BeatmapEventType, Dictionary<float, Color>> CustomLightColours = new Dictionary<BeatmapEventType, Dictionary<float, Color>>();

        // Creates dictionary loaded with all _lightRGB custom events and indexs them with the event's time and type
        public static void Activate(List<CustomEventData> eventData)
        {
            if (!ChromaBehaviour.LightingRegistered) return;
            foreach (CustomEventData d in eventData)
            {
                try
                {
                    dynamic dynData = d.data;
                    int id = (int)Trees.at(dynData, "_event");
                    List<object> color = Trees.at(dynData, "_color");
                    float r = Convert.ToSingle(color[0]);
                    float g = Convert.ToSingle(color[1]);
                    float b = Convert.ToSingle(color[2]);

                    Color c = new Color(r, g, b);
                    if (color.Count > 3) c = c.ColorWithAlpha(Convert.ToSingle(color[3]));

                    // Dictionary of dictionaries!
                    Dictionary<float, Color> dictionaryID;
                    if (!CustomLightColours.TryGetValue((BeatmapEventType)id, out dictionaryID))
                    {
                        dictionaryID = new Dictionary<float, Color>();
                        CustomLightColours.Add((BeatmapEventType)id, dictionaryID);
                    }
                    dictionaryID.Add(d.time, c);

                    ColourManager.TechnicolourLightsForceDisabled = true;
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