using Chroma.Utils;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Events
{
    internal class ChromaLightColourEvent
    {
        internal static Dictionary<BeatmapEventType, Dictionary<float, Color>> CustomLightColours = new Dictionary<BeatmapEventType, Dictionary<float, Color>>();

        // Creates dictionary loaded with all _lightRGB custom events and indexs them with the event's time and type
        internal static void Activate(List<CustomEventData> eventData)
        {
            foreach (CustomEventData d in eventData)
            {
                try
                {
                    dynamic dynData = d.data;
                    int id = (int)Trees.at(dynData, "_event");
                    Color c = ChromaUtils.GetColorFromData(dynData);

                    // Dictionary of dictionaries!
                    if (!CustomLightColours.TryGetValue((BeatmapEventType)id, out Dictionary<float, Color> dictionaryID))
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