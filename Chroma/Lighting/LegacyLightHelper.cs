using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chroma.Lighting;

// Please let me delete this whole class
internal class LegacyLightHelper
{
    internal const int RGB_INT_OFFSET = 2000000000;

    internal LegacyLightHelper(IEnumerable<BasicBeatmapEventData> eventData)
    {
        foreach (BasicBeatmapEventData d in eventData)
        {
            if (d.value < RGB_INT_OFFSET)
            {
                continue;
            }

            if (!LegacyColorEvents.TryGetValue(d.basicBeatmapEventType, out List<Tuple<float, Color>> dictionaryID))
            {
                dictionaryID = [];
                LegacyColorEvents.Add(d.basicBeatmapEventType, dictionaryID);
            }

            dictionaryID.Add(new Tuple<float, Color>(d.time, ColorFromInt(d.value)));
        }
    }

    internal Dictionary<BasicBeatmapEventType, List<Tuple<float, Color>>> LegacyColorEvents { get; } = new();

    internal Color? GetLegacyColor(BasicBeatmapEventData beatmapEventData)
    {
        if (!LegacyColorEvents.TryGetValue(
                beatmapEventData.basicBeatmapEventType,
                out List<Tuple<float, Color>> dictionaryID))
        {
            return null;
        }

        List<Tuple<float, Color>> colors = dictionaryID.Where(n => n.Item1 <= beatmapEventData.time).ToList();
        if (colors.Count > 0)
        {
            return colors.Last().Item2;
        }

        return null;
    }

    private static Color ColorFromInt(int rgb)
    {
        rgb -= RGB_INT_OFFSET;
        int red = (rgb >> 16) & 0x0ff;
        int green = (rgb >> 8) & 0x0ff;
        int blue = rgb & 0x0ff;
        return new Color(red / 255f, green / 255f, blue / 255f);
    }
}
