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
        internal float dissolve = 1;
        internal float dissolveArrow = 1;

        internal Vector3 position;
        internal Vector3 rotation;
        internal Vector3 scale = Vector3.one;
        internal Vector3 localRotation;

        internal Vector3 visualPosition;
        internal Vector3 visualRotation;
        internal Vector3 visualScale = Vector3.one;
        internal Vector3 visualLocalRotation;

        internal PointData definePosition;
        internal PointData defineRotation;
        internal PointData defineScale;
        internal PointData defineLocalRotation;

        internal PointData definitePosition;
    }
}