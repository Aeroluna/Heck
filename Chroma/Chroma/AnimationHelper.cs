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

        internal static void GetColorOffset(PointDefinition localColor, Track track, float time, out Color? color)
        {
            Vector4? pathColor = localColor?.InterpolateVector4(time) ?? TryGetVector4PathProperty(track, COLOR, time);

            Vector4? colorVector = MultVector4Nullables((Vector4?)TryGetPropertyAsObject(track, COLOR), pathColor);

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

            // For Environment Enhancements
            properties.Add("_localPosition", new Property(PropertyType.Vector3));
        }
    }
}
