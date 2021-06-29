namespace Chroma
{
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

        internal static void GetColorOffset(PointDefinition? localColor, Track? track, float time, out Color? color)
        {
            Vector4? pathColor = localColor?.InterpolateVector4(time) ?? TryGetVector4PathProperty(track, COLOR, time);

            Vector4? colorVector = MultVector4Nullables((Vector4?)TryGetProperty(track, COLOR), pathColor);

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
