namespace Chroma
{
    using System.Collections.Generic;
    using System.Linq;
    using Heck.Animation;
    using UnityEngine;
    using static Heck.Animation.AnimationHelper;
    using static Heck.NullableExtensions;
    using static Plugin;

    internal static class AnimationHelper
    {
        internal static void GetColorOffset(PointDefinition? localColor, IEnumerable<Track>? tracks, float time, out Color? color)
        {
            Vector4? pathColor = localColor?.InterpolateVector4(time);
            Vector4? colorVector = null;
            if (tracks != null)
            {
                if (tracks.Count() > 1)
                {
                    pathColor ??= MultVector4Nullables(tracks.Select(n => TryGetVector4PathProperty(n, COLOR, time)));
                    colorVector = MultVector4Nullables(MultVector4Nullables(tracks.Select(n => TryGetProperty<Vector4?>(n, COLOR))), pathColor);
                }
                else
                {
                    Track track = tracks.First();
                    pathColor ??= TryGetVector4PathProperty(track, COLOR, time);
                    colorVector = MultVector4Nullables(TryGetProperty<Vector4?>(track, COLOR), pathColor);
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
            track.AddProperty(COLOR, PropertyType.Vector4);
            track.AddPathProperty(COLOR, PropertyType.Vector4);

            // For Environment Enhancements
            track.AddProperty(POSITION, PropertyType.Vector3);
            track.AddProperty(LOCALPOSITION, PropertyType.Vector3);
            track.AddProperty(OBJECTROTATION, PropertyType.Quaternion);
            track.AddProperty(LOCALROTATION, PropertyType.Quaternion);
            track.AddProperty(SCALE, PropertyType.Vector3);

            // For Fog Control
            track.AddProperty(ATTENUATION, PropertyType.Linear);
            track.AddProperty(OFFSET, PropertyType.Linear);
            track.AddProperty(HEIGHTFOGSTARTY, PropertyType.Linear);
            track.AddProperty(HEIGHTFOGHEIGHT, PropertyType.Linear);
        }
    }
}
