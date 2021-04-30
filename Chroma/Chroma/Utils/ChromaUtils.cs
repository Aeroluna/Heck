namespace Chroma.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using static Plugin;

    internal static class ChromaUtils
    {
        internal static Color? GetColorFromData(dynamic data, string member = COLOR)
        {
            IEnumerable<float> color = ((List<object>)CustomJSONData.Trees.at(data, member))?.Select(n => Convert.ToSingle(n));
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
    }
}
