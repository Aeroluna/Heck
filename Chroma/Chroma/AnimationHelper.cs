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
        internal static void SubscribeColorEvents()
        {
            TrackBuilder.TrackCreated += OnTrackCreated;
        }

        internal static void GetColorOffset(PointDefinition? localColor, IEnumerable<Track>? tracks, float time, out Color? color)
        {
            Vector3? pathColor = localColor?.Interpolate(time);
            Vector4? colorVector = null;
            if (tracks != null)
            {
                pathColor ??= MultVector4Nullables(tracks.Select(n => TryGetVector4PathProperty(n, COLOR, time)).ToArray());
                Vector4? trackColor = MultVector4Nullables(tracks.Select(n => TryGetProperty<Vector4?>(n, COLOR)).ToArray());
                colorVector = MultVector4Nullables(trackColor, pathColor);
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

        private static void OnTrackCreated(Track track)
        {
            track.AddProperty(COLOR, PropertyType.Vector4);
            track.AddPathProperty(COLOR, PropertyType.Vector4);

            // For Environment Enhancements
            track.AddProperty(POSITION, PropertyType.Vector3);
            track.AddProperty(LOCALPOSITION, PropertyType.Vector3);
            track.AddProperty(OBJECTROTATION, PropertyType.Quaternion);
            track.AddProperty(LOCALROTATION, PropertyType.Quaternion);
            track.AddProperty(SCALE, PropertyType.Vector3);
        }
    }
}
