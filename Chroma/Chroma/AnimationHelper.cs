namespace Chroma
{
    using System.Collections.Generic;
    using NoodleExtensions.Animation;
    using UnityEngine;
    using static NoodleExtensions.Animation.AnimationHelper;
    using static NoodleExtensions.NullableExtensions;
    using static Plugin;

    internal static class AnimationHelper
    {
        internal static void SubscribeColorEvents()
        {
            TrackManager.TrackCreated += OnTrackCreated;
        }

        internal static void GetColorOffset(dynamic customData, Track track, float time, out Color? color)
        {
            TryGetPointData(customData, COLOR, out PointDefinition localColor);

            Vector4? pathColor = localColor?.InterpolateVector4(time) ?? TryGetPathProperty(track, COLOR, time);

            Vector4? colorVector = MultVector4Nullables(TryGetProperty(track, COLOR), pathColor);

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
            properties.Add(COLOR, new Property(PropertyType.Vector4));

            IDictionary<string, Property> pathProperties = track.PathProperties;
            pathProperties.Add(COLOR, new Property(PropertyType.Vector4));
        }
    }
}
