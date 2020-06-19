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
        // TODO: maybe properly parent notes/obstacles to the track
        internal Vector3? _position;

        internal Quaternion? _rotation;
        internal Vector3? _scale;
        internal Quaternion? _localRotation;
        internal float? _dissolve;
        internal float? _dissolveArrow;

        internal float _pathInterpolationTime = 0;
        internal PointDataInterpolation _pathPosition;
        internal PointDataInterpolation _pathRotation;
        internal PointDataInterpolation _pathScale;
        internal PointDataInterpolation _pathLocalRotation;
        internal PointDataInterpolation _pathDefinitePosition; // TODO: fix definiteposition
        internal PointDataInterpolation _pathDissolve;
        internal PointDataInterpolation _pathDissolveArrow;

        internal Track()
        {
            _pathPosition = new PointDataInterpolation(this);
            _pathRotation = new PointDataInterpolation(this);
            _pathScale = new PointDataInterpolation(this);
            _pathLocalRotation = new PointDataInterpolation(this);
            _pathDefinitePosition = new PointDataInterpolation(this);
            _pathDissolve = new PointDataInterpolation(this);
            _pathDissolveArrow = new PointDataInterpolation(this);
        }

        internal void ResetVariables()
        {
            _position = null;
            _rotation = null;
            _scale = null;
            _localRotation = null;
            _dissolve = null;
            _dissolveArrow = null;
            _pathInterpolationTime = 0;
            _pathPosition = new PointDataInterpolation(this);
            _pathRotation = new PointDataInterpolation(this);
            _pathScale = new PointDataInterpolation(this);
            _pathLocalRotation = new PointDataInterpolation(this);
            _pathDefinitePosition = new PointDataInterpolation(this);
            _pathDissolve = new PointDataInterpolation(this);
            _pathDissolveArrow = new PointDataInterpolation(this);
        }
    }
}
