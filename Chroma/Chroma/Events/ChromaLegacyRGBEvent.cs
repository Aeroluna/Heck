namespace Chroma.Events
{
    using System.Collections.Generic;
    using UnityEngine;

    internal class ChromaLegacyRGBEvent
    {
        internal const int RGB_INT_OFFSET = 2000000000;

        internal static Dictionary<BeatmapEventType, List<TimedColor>> LightColors { get; } = new Dictionary<BeatmapEventType, List<TimedColor>>();

        internal static void Activate(BeatmapEventData[] eventData)
        {
            foreach (BeatmapEventData d in eventData)
            {
                if (d.value >= RGB_INT_OFFSET)
                {
                    // Luckily I already had a system in place to replicate this functionality
                    if (!LightColors.TryGetValue(d.type, out List<TimedColor> dictionaryID))
                    {
                        dictionaryID = new List<TimedColor>();
                        LightColors.Add(d.type, dictionaryID);
                    }

                    dictionaryID.Add(new TimedColor(d.time, ColorFromInt(d.value)));
                }
            }
        }

        private static Color ColorFromInt(int rgb)
        {
            rgb = rgb - RGB_INT_OFFSET;
            int red = (rgb >> 16) & 0x0ff;
            int green = (rgb >> 8) & 0x0ff;
            int blue = rgb & 0x0ff;
            return new Color(red / 255f, green / 255f, blue / 255f);
        }
    }
}
