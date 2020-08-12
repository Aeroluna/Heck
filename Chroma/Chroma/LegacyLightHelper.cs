namespace Chroma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    // Please let me delete this whole class
    internal class LegacyLightHelper
    {
        internal const int RGB_INT_OFFSET = 2000000000;

        internal static IDictionary<BeatmapEventType, List<Tuple<float, Color>>> LegacyColorEvents { get; } = new Dictionary<BeatmapEventType, List<Tuple<float, Color>>>();

        internal static void Activate(BeatmapEventData[] eventData)
        {
            LegacyColorEvents.Clear();
            foreach (BeatmapEventData d in eventData)
            {
                if (d.value >= RGB_INT_OFFSET)
                {
                    if (!LegacyColorEvents.TryGetValue(d.type, out List<Tuple<float, Color>> dictionaryID))
                    {
                        dictionaryID = new List<Tuple<float, Color>>();
                        LegacyColorEvents.Add(d.type, dictionaryID);
                    }

                    dictionaryID.Add(new Tuple<float, Color>(d.time, ColorFromInt(d.value)));
                }
            }
        }

        internal static Color? GetLegacyColor(BeatmapEventData beatmapEventData)
        {
            if (LegacyColorEvents.TryGetValue(beatmapEventData.type, out List<Tuple<float, Color>> dictionaryID))
            {
                List<Tuple<float, Color>> colors = dictionaryID.Where(n => n.Item1 <= beatmapEventData.time).ToList();
                if (colors.Count > 0)
                {
                    return colors.Last().Item2;
                }
            }

            return null;
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
