namespace NoodleExtensions.Animation
{
    using System;
    using System.Collections.Generic;

    public class TrackManager
    {
        public static event Action<Track> TrackWasCreated;

        internal Dictionary<string, Track> Tracks { get; private set; } = new Dictionary<string, Track>();

        internal Track AddToTrack(string trackName)
        {
            Track track;
            if (!Tracks.TryGetValue(trackName, out track))
            {
                track = new Track();
                TrackWasCreated?.Invoke(track);
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
