using System.Collections.Generic;
using JetBrains.Annotations;
using Zenject;

namespace Heck.Animation
{
    internal class TrackUpdateManager : ILateTickable
    {
        private readonly HashSet<Track> _tracks;

        [UsedImplicitly]
        private TrackUpdateManager(Dictionary<string, Track> beatmapTracks)
        {
            _tracks = new HashSet<Track>(beatmapTracks.Values);
        }

        public void LateTick()
        {
            foreach (Track track in _tracks)
            {
                track.UpdatedThisFrame = false;
            }
        }
    }
}
