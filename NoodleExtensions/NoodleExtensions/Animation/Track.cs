namespace NoodleExtensions.Animation
{
    using System;
    using System.Collections.Generic;
    using CustomJSONData.CustomBeatmap;

    public class TrackManager
    {
        internal TrackManager(CustomBeatmapData beatmapData)
        {
            TrackManagerCreated?.Invoke(this, beatmapData);
        }

        public static event EventHandler<CustomBeatmapData> TrackManagerCreated;

        public static event Action<Track> TrackCreated;

        public IDictionary<string, Track> Tracks { get; private set; } = new Dictionary<string, Track>();

        public Track AddTrack(string trackName)
        {
            Track track;
            if (!Tracks.TryGetValue(trackName, out track))
            {
                track = new Track();
                TrackCreated?.Invoke(track);
                track.ResetVariables();
                Tracks.Add(trackName, track);
            }

            return track;
        }
    }

    public class Track
    {
        public IDictionary<string, Property> Properties { get; } = new Dictionary<string, Property>();

        public IDictionary<string, Property> PathProperties { get; } = new Dictionary<string, Property>();

        internal void ResetVariables()
        {
            foreach (KeyValuePair<string, Property> valuePair in Properties)
            {
                valuePair.Value.Value = null;
            }

            foreach (KeyValuePair<string, Property> valuePair in PathProperties)
            {
                valuePair.Value.Value = new PointDefinitionInterpolation();
            }
        }
    }
}
