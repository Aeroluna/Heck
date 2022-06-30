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
    internal class EventController : IDisposable
    {
        private readonly BeatmapCallbacksController _callbacksController;
        private readonly DeserializedData _deserializedData;
        private readonly LazyInject<ParentController> _parentController;
        private readonly LazyInject<PlayerTrack> _playerTrack;
        private readonly BeatmapDataCallbackWrapper? _callbackWrapper;

        [UsedImplicitly]
        private EventController(
            BeatmapCallbacksController callbacksController,
            [Inject(Id = ID)] DeserializedData deserializedData,
            LazyInject<ParentController> parentController,
            LazyInject<PlayerTrack> playerTrack)
        {
            _deserializedData = deserializedData;
            _parentController = parentController;
            _playerTrack = playerTrack;
            _callbacksController = callbacksController;
            _callbackWrapper = callbacksController.AddBeatmapCallback<CustomEventData>(HandleCallback);
        }

        public void Dispose()
        {
            _callbacksController.RemoveBeatmapCallback(_callbackWrapper);
        }

        private void HandleCallback(CustomEventData customEventData)
        {
            switch (customEventData.eventType)
            {
                case ASSIGN_TRACK_PARENT:
                    _parentController.Value.Create(customEventData);
                    break;

                case ASSIGN_PLAYER_TO_TRACK:
                    if (_deserializedData.Resolve(customEventData, out NoodlePlayerTrackEventData? noodlePlayerData))
                    {
                        _playerTrack.Value.AssignTrack(noodlePlayerData.Track);
                    }

                    break;
            }
        }
    }
}
