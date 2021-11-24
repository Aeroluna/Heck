using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using SongCore;
using UnityEngine;
using static Chroma.ChromaController;

namespace Chroma.Utils
{
    internal static class ChromaUtils
    {
        internal static Color? GetColorFromData(Dictionary<string, object?> data, string member = COLOR)
        {
            List<float>? color = data.Get<List<object>>(member)?.Select(Convert.ToSingle).ToList();
            if (color == null)
            {
                return null;
            }

            return new Color(color[0], color[1], color[2], color.Count > 3 ? color[3] : 1);
        }

        internal static void SetSongCoreCapability(string capability, bool enabled = true)
        {
            if (enabled)
            {
                Collections.RegisterCapability(capability);
            }
            else
            {
                Collections.DeregisterizeCapability(capability);
            }
        }

        internal static Color MultAlpha(this Color color, float alpha)
        {
            return color.ColorWithAlpha(color.a * alpha);
        }
    }
}
