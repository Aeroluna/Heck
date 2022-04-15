using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using JetBrains.Annotations;
using Zenject;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.Animation
{
    [UsedImplicitly]
    internal class EventController : IDisposable
    {
        private readonly BeatmapCallbacksController? _callbacksController;
        private readonly CustomData _customData;
        private readonly LazyInject<ParentController> _parentController;
        private readonly LazyInject<PlayerTrack> _playerTrack;
        private readonly BeatmapDataCallbackWrapper? _callbackWrapper;

        private EventController(
            BeatmapCallbacksController callbacksController,
            [Inject(Id = ID)] CustomData customData,
            LazyInject<ParentController> parentController,
            LazyInject<PlayerTrack> playerTrack)
        {
            _customData = customData;
            _parentController = parentController;
            _playerTrack = playerTrack;
            _callbacksController = callbacksController;
            _callbackWrapper = callbacksController.AddBeatmapCallback<CustomEventData>(HandleCallback);
        }

        public void Dispose()
        {
            _callbacksController?.RemoveBeatmapCallback(_callbackWrapper);
        }

        private void HandleCallback(CustomEventData customEventData)
        {
            switch (customEventData.eventType)
            {
                case ASSIGN_TRACK_PARENT:
                    if (!_customData.Resolve(customEventData, out NoodleParentTrackEventData? noodleParentData))
                    {
                        return;
                    }

                    List<Track> tracks = noodleParentData.ChildrenTracks;
                    Track parentTrack = noodleParentData.ParentTrack;
                    _parentController.Value.Create(
                        tracks,
                        parentTrack,
                        noodleParentData.WorldPositionStays,
                        noodleParentData.Position,
                        noodleParentData.Rotation,
                        noodleParentData.LocalRotation,
                        noodleParentData.Scale);
                    break;
                case ASSIGN_PLAYER_TO_TRACK:
                    if (_customData.Resolve(customEventData, out NoodlePlayerTrackEventData? noodlePlayerData))
                    {
                        _playerTrack.Value.AssignTrack(noodlePlayerData.Track);
                    }

                    break;
            }
        }
    }
}
