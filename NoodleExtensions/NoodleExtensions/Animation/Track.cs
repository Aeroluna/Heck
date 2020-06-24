using System.Collections.Generic;
using System;
using System.Dynamic;
using UnityEngine;

namespace NoodleExtensions.Animation
{
    public class TrackManager
    {
        public static event Action<Track> trackWasCreated;

        internal Dictionary<string, Track> _tracks { get; private set; } = new Dictionary<string, Track>();

        internal Track AddToTrack(string trackName)
        {
            Track track;
            if (!_tracks.TryGetValue(trackName, out track))
            {
                track = new Track();
                trackWasCreated?.Invoke(track);
                _tracks.Add(trackName, track);
            }
            return track;
        }
    }

    public class Track
    {
        public IDictionary<string, Property> _properties = new Dictionary<string, Property>();
        public IDictionary<string, Property> _pathProperties = new Dictionary<string, Property>();

        internal void ResetVariables()
        {
            foreach (KeyValuePair<string, Property> valuePair in _properties)
            {
                valuePair.Value._property = null;
            }
            foreach (KeyValuePair<string, Property> valuePair in _pathProperties)
            {
                valuePair.Value._property = new PointDataInterpolation();
            }
        }
    }
}
