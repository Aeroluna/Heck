using System.Collections.Generic;
using System.Linq;
using Heck.Animation;
using UnityEngine;
using static Chroma.ChromaController;
using static Heck.NullableExtensions;

namespace Chroma.Animation
{
    internal static class AnimationHelper
    {
        internal static void GetColorOffset(PointDefinition? localColor, List<Track>? tracks, float time, out Color? color)
        {
            Vector4? pathColor = localColor?.InterpolateVector4(time);
            Vector4? colorVector;
            if (tracks != null)
            {
                if (tracks.Count > 1)
                {
                    pathColor ??= MultVector4Nullables(tracks.Select(n => n.GetVector4PathProperty(COLOR, time)));
                    colorVector = MultVector4Nullables(MultVector4Nullables(tracks.Select(n => n.GetProperty<Vector4?>(COLOR))), pathColor);
                }
                else
                {
                    Track track = tracks.First();
                    pathColor ??= track.GetVector4PathProperty(COLOR, time);
                    colorVector = MultVector4Nullables(track.GetProperty<Vector4?>(COLOR), pathColor);
                }
            }
            else
            {
                colorVector = pathColor;
            }

            if (colorVector.HasValue)
            {
                Vector4 vectorValue = colorVector.Value;
                color = new Color(vectorValue.x, vectorValue.y, vectorValue.z, vectorValue.w);
            }
            else
            {
                color = null;
            }
        }

        internal static void OnTrackCreated(Track track)
        {
            track.AddProperty(COLOR, PropertyType.Vector4, V2_COLOR);
            track.AddPathProperty(COLOR, PropertyType.Vector4, V2_COLOR);

            // For Fog Control
            track.AddProperty(ATTENUATION, PropertyType.Linear, V2_ATTENUATION);
            track.AddProperty(OFFSET, PropertyType.Linear, V2_OFFSET);
            track.AddProperty(HEIGHT_FOG_STARTY, PropertyType.Linear, V2_HEIGHT_FOG_STARTY);
            track.AddProperty(HEIGHT_FOG_HEIGHT, PropertyType.Linear, V2_HEIGHT_FOG_HEIGHT);
        }
    }
}
