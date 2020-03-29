using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Events
{
    internal class ChromaLegacyRGBEvent
    {
        internal const int RGB_INT_OFFSET = 2000000000;

        private static Color ColourFromInt(int rgb)
        {
            rgb = rgb - RGB_INT_OFFSET;
            int red = (rgb >> 16) & 0x0ff;
            int green = (rgb >> 8) & 0x0ff;
            int blue = (rgb) & 0x0ff;
            return new Color(red / 255f, green / 255f, blue / 255f);
        }

        internal static void Activate(BeatmapEventData[] eventData)
        {
            foreach (BeatmapEventData d in eventData)
            {
                if (d.value >= RGB_INT_OFFSET)
                {
                    // Luckily I already had a system in place to replicate this functionality
                    if (!ChromaLightColourEvent.CustomLightColours.TryGetValue(d.type, out Dictionary<float, Color> dictionaryID))
                    {
                        dictionaryID = new Dictionary<float, Color>();
                        ChromaLightColourEvent.CustomLightColours.Add(d.type, dictionaryID);
                    }
                    dictionaryID.Add(d.time, ColourFromInt(d.value));
                }
            }
        }
    }
}