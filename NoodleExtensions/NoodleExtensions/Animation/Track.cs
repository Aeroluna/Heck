using System.Collections.Generic;
using UnityEngine;

namespace NoodleExtensions.Animation
{
    internal class TrackManager
    {
        internal Dictionary<string, Track> _tracks { get; private set; } = new Dictionary<string, Track>();

        internal Track AddToTrack(string trackName, BeatmapObjectData noteData)
        {
            Track track;
            if (!_tracks.TryGetValue(trackName, out track))
            {
                track = new Track();
                _tracks.Add(trackName, track);
            }
            return track;
        }
    }

    internal class Track
    {
        internal float dissolve = 0;
        internal float dissolveArrow = 0;

        internal Vector3 position;
        internal Vector3 rotation;
        internal Vector3 scale = Vector3.one;
        internal Vector3 localRotation;

        internal PointData pathPosition;
        internal PointData pathRotation;
        internal PointData pathScale;
        internal PointData pathLocalRotation;

        internal PointData definitePosition;
    }
}