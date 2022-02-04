using System.Collections.Generic;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.Animation
{
    internal class EventController : MonoBehaviour
    {
        private CustomData _customData = null!;
        private LazyInject<ParentController> _parentController = null!;
        private LazyInject<PlayerTrack> _playerTrack = null!;

        [UsedImplicitly]
        [Inject]
        internal void Construct(
            CustomEventCallbackController customEventCallbackController,
            [Inject(Id = ID)] CustomData customData,
            LazyInject<ParentController> parentController,
            LazyInject<PlayerTrack> playerTrack,
            [Inject(Id = "isMultiplayer")] bool isMultiplayer)
        {
            if (isMultiplayer)
            {
                return;
            }

            _customData = customData;
            _parentController = parentController;
            _playerTrack = playerTrack;
            customEventCallbackController.AddCustomEventCallback(HandleCallback);
        }

        private void HandleCallback(CustomEventData customEventData)
        {
            switch (customEventData.type)
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
