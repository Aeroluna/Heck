namespace Chroma.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using UnityEngine;
    using static Chroma.Plugin;

    internal static class ChromaUtils
    {
        internal static Color? GetColorFromData(Dictionary<string, object?> data, string member = COLOR)
        {
            IEnumerable<float>? color = data.Get<List<object>>(member)?.Select(n => Convert.ToSingle(n));
            if (color == null)
            {
                return null;
            }

            return new Color(color.ElementAt(0), color.ElementAt(1), color.ElementAt(2), color.Count() > 3 ? color.ElementAt(3) : 1);
        }

        internal static void SetSongCoreCapability(string capability, bool enabled = true)
        {
            if (enabled)
            {
                SongCore.Collections.RegisterCapability(capability);
            }
            else
            {
                SongCore.Collections.DeregisterizeCapability(capability);
            }
        }

        internal static Color MultAlpha(this Color color, float alpha)
        {
            return color.ColorWithAlpha(color.a * alpha);
        }
    }
}
