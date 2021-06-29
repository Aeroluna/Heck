namespace Heck.Animation
{
    using System;
    using System.Collections.Generic;
    using CustomJSONData.CustomBeatmap;

    public class TrackBuilder
    {
        internal TrackBuilder(CustomBeatmapData beatmapData)
        {
            TrackManagerCreated?.Invoke(this, beatmapData);
        }

        public static event Action<TrackBuilder, CustomBeatmapData>? TrackManagerCreated;

        public static event Action<Track>? TrackCreated;

        public IDictionary<string, Track> Tracks { get; } = new Dictionary<string, Track>();

        public Track AddTrack(string trackName)
        {
            if (!Tracks.TryGetValue(trackName, out Track track))
            {
                track = new Track();
                TrackCreated?.Invoke(track);
                Tracks.Add(trackName, track);
            }

            return track;
        }
    }

    public class Track
    {
        internal IDictionary<string, Property> Properties { get; } = new Dictionary<string, Property>();

        internal IDictionary<string, Property> PathProperties { get; } = new Dictionary<string, Property>();

        public void AddProperty(string name, PropertyType propertyType)
        {
            if (Properties.ContainsKey(name))
            {
                ////Plugin.Logger.Log($"Duplicate property {name}, skipping...");
            }
            else
            {
                Properties.Add(name, new Property(propertyType));
            }
        }

        public void AddPathProperty(string name, PropertyType propertyType)
        {
            if (PathProperties.ContainsKey(name))
            {
                ////Plugin.Logger.Log($"Duplicate path property {name}, skipping...");
            }
            else
            {
                PathProperties.Add(name, new PathProperty(propertyType));
            }
        }
    }
}
