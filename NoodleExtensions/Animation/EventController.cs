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
        private readonly DeserializedData _deserializedData;
        private readonly LazyInject<ParentController> _parentController;
        private readonly PlayerTrack.PlayerTrackFactory _playerTrackFactory;
        private readonly BeatmapDataCallbackWrapper? _callbackWrapper;
        private readonly Dictionary<PlayerTrackObject, PlayerTrack> _playerTracks = new();

        private EventController(
            BeatmapCallbacksController callbacksController,
            [Inject(Id = ID)] DeserializedData deserializedData,
            LazyInject<ParentController> parentController,
            PlayerTrack.PlayerTrackFactory playerTrackFactory)
        {
            _deserializedData = deserializedData;
            _parentController = parentController;
            _playerTrackFactory = playerTrackFactory;
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
                    if (!_deserializedData.Resolve(customEventData, out NoodleParentTrackEventData? noodleParentData))
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
                    if (_deserializedData.Resolve(customEventData, out NoodlePlayerTrackEventData? noodlePlayerData))
                    {
                        PlayerTrackObject resultPlayerTrackObject = noodlePlayerData.PlayerTrackObject;
                        if (!_playerTracks.TryGetValue(resultPlayerTrackObject, out PlayerTrack? playerTrack))
                        {
                            _playerTracks[resultPlayerTrackObject] = playerTrack =
                                _playerTrackFactory.Create(resultPlayerTrackObject);
                        }

                        playerTrack.AssignTrack(noodlePlayerData.Track);
                    }

                    break;
            }
        }
    }
}
