namespace Chroma
{
    using System.Collections.Generic;
    using NoodleExtensions.Animation;
    using UnityEngine;
    using static NoodleExtensions.Animation.AnimationHelper;
    using static NoodleExtensions.NullableExtensions;

    internal static class AnimationHelper
    {
        internal static void SubscribeColorEvents()
        {
            TrackManager.TrackCreated += OnTrackCreated;
        }

        internal static void GetColorOffset(dynamic customData, Track track, float time, out Color? color)
        {
            TryGetPointData(customData, "_color", out PointDefinition localColor);

            Vector4? pathColor = localColor?.InterpolateVector4(time) ?? TryGetPathProperty(track, "_color", time);

            Vector4? colorVector = MultVector4Nullables(TryGetProperty(track, "_color"), pathColor);

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
            IDictionary<string, Property> properties = track.Properties;
            properties.Add("_color", new Property(PropertyType.Vector4));

            IDictionary<string, Property> pathProperties = track.PathProperties;
            pathProperties.Add("_color", new Property(PropertyType.Vector4));
        }
    }
}
