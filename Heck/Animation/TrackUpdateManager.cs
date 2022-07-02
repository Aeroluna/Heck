using System.Collections.Generic;
using HarmonyLib;
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
            _tracks.Do(n => n.UpdatedThisFrame = false);
        }
    }
}
