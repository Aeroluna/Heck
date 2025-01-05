using System.Collections.Generic;
using JetBrains.Annotations;
using Zenject;

namespace Heck.Animation;

internal class TrackUpdateManager : ILateTickable
{
    private readonly AudioTimeSyncController _audioTimeSyncController;
    private readonly HashSet<Track> _tracks;

    private bool _songStarted;

    [UsedImplicitly]
    private TrackUpdateManager(Dictionary<string, Track> beatmapTracks, AudioTimeSyncController audioTimeSyncController)
    {
        _tracks = [..beatmapTracks.Values];
        _audioTimeSyncController = audioTimeSyncController;
        audioTimeSyncController.stateChangedEvent += OnSongStarted;
    }

    public void LateTick()
    {
        if (!_songStarted)
        {
            return;
        }

        foreach (Track track in _tracks)
        {
            track.UpdatedThisFrame = false;
        }
    }

    private void OnSongStarted()
    {
        _audioTimeSyncController.stateChangedEvent -= OnSongStarted;
        _songStarted = true;
    }
}
