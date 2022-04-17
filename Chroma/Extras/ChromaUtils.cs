using SongCore;
using UnityEngine;

namespace Chroma.Extras
{
    internal static class ChromaUtils
    {
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
