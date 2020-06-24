using Chroma.Utils;
using CustomJSONData.CustomBeatmap;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Events
{
    internal class ChromaBombColourEvent
    {
        internal static List<TimedColor> BombColours = new List<TimedColor>();

        // Creates dictionary loaded with all _bombColor custom events and indexs them with the event's time
        internal static void Activate(List<CustomEventData> eventData)
        {
            foreach (CustomEventData d in eventData)
            {
                try
                {
                    dynamic dynData = d.data;
                    Color c = ChromaUtils.GetColorFromData(dynData, false);
                    BombColours.Add(new TimedColor(d.time, c));

                    ColourManager.TechnicolourBombsForceDisabled = true;
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