using System;
using System.Collections.Generic;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using Heck.Animation.Transform;
using Heck.Deserialize;
using Heck.Event;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static Heck.HeckController;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.Animation
{
    internal enum PlayerTrackObject
    {
        Root,
        Head,
        LeftHand,
        RightHand
    }

    internal class PlayerTrack : MonoBehaviour
    {
        // because camera2 is cringe
        // stop using reflection you jerk
        [UsedImplicitly]
        private static PlayerTrack? _instance;

        [UsedImplicitly]
        private Transform _transform = null!;

        private bool _leftHanded;
        private bool _v2;

        private Vector3 _startPos = Vector3.zero;
        private Quaternion _startLocalRot = Quaternion.identity;

        private Track? _track;
        private PauseController? _pauseController;
        private MultiplayerPlayersManager? _multiPlayersManager;
        private MultiplayerOutroAnimationController? _multiOutroController;

        private TransformController? _transformController;
        private TransformControllerFactory _transformFactory = null!;

        private GameObject _multiplayerPositioner = null!;

        private PlayerTrackObject _target;

        internal void AssignTrack(
            Track track)
        {
            _track?.RemoveGameObject(gameObject);

            _track = track;

            track.AddGameObject(gameObject);

            if (_v2)
            {
                return;
            }

            _transformController = _transformFactory.Create(gameObject, _track, true);
        }

        [UsedImplicitly]
        [Inject]
        private void Construct(
            IReadonlyBeatmapData beatmapData,
            [Inject(Id = LEFT_HANDED_ID)] bool leftHanded,
            TransformControllerFactory transformControllerFactory,
            [InjectOptional] PauseController? pauseController,
            [InjectOptional] PauseMenuManager? pauseMenuManager,
            [InjectOptional] MultiplayerLocalActivePlayerInGameMenuController? multiMenuController,
            [InjectOptional] MultiplayerPlayersManager? multiPlayersManager,
            [InjectOptional] MultiplayerOutroAnimationController? multiOutroController,
            PlayerTrackObject target)
        {
            if (pauseController != null)
            {
                pauseController.didPauseEvent += OnDidPauseEvent;
                pauseController.didResumeEvent += OnDidResumeEvent;
            }

            Transform origin = transform;
            _startLocalRot = origin.localRotation;
            _startPos = origin.localPosition;
            _leftHanded = leftHanded;
            _transformFactory = transformControllerFactory;
            _target = target;

            if (target == PlayerTrackObject.Root)
            {
                _pauseController = pauseController;
                _multiPlayersManager = multiPlayersManager;
                _multiOutroController = multiOutroController;

                if (pauseMenuManager != null)
                {
                    pauseMenuManager.transform.SetParent(origin, false);
                }

                if (multiMenuController != null)
                {
                    multiMenuController.transform.SetParent(origin, false);
                }
            }

            // v3 uses an underlying TransformController
            _v2 = ((CustomBeatmapData)beatmapData).version.IsVersion2();
            if (!_v2)
            {
                enabled = false;
            }

            // cam2 is cringe cam2 is cringe cam2 is cringe
            // ReSharper disable once InvertIf
            if (target == PlayerTrackObject.Head)
            {
                _instance = this;
                _transform = origin;
            }
        }

        private void OnDidPauseEvent()
        {
            if (_target != PlayerTrackObject.Root)
            {
                Transform transform1 = transform;
                transform1.localPosition = _startPos;
                transform1.localRotation = _startLocalRot;
            }

            if (_v2)
            {
                enabled = false;
            }

            if (_transformController != null)
            {
                _transformController.enabled = false;
            }
        }

        private void OnDidResumeEvent()
        {
            if (_v2)
            {
                enabled = true;
            }

            if (_transformController != null)
            {
                _transformController.enabled = true;
            }
        }

        private void OnDestroy()
        {
            if (_pauseController != null)
            {
                _pauseController.didPauseEvent -= OnDidPauseEvent;
            }

            if (_transformController != null)
            {
                Destroy(_transformController);
            }
        }

        private void Start()
        {
            if (_multiPlayersManager == null)
            {
                return;
            }

            _multiplayerPositioner = new GameObject();
            _multiplayerPositioner.transform.SetParent(transform);
        }

        private void Update()
        {
            if (_track == null)
            {
                return;
            }

            Quaternion? rotation = _track.GetProperty<Quaternion>(OFFSET_ROTATION)?.Mirror(_leftHanded);
            Vector3? position = _track.GetProperty<Vector3>(OFFSET_POSITION)?.Mirror(_leftHanded);

            Quaternion worldRotationQuatnerion = Quaternion.identity;
            Vector3 positionVector = _startPos;
            if (rotation.HasValue || position.HasValue)
            {
                Quaternion finalRot = rotation ?? Quaternion.identity;
                worldRotationQuatnerion *= finalRot;
                Vector3 finalPos = position ?? Vector3.zero;
                positionVector = worldRotationQuatnerion * ((finalPos * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance) + _startPos);
            }

            worldRotationQuatnerion *= _startLocalRot;
            Quaternion? localRotation = _track.GetProperty<Quaternion>(LOCAL_ROTATION)?.Mirror(_leftHanded);
            if (localRotation.HasValue)
            {
                worldRotationQuatnerion *= localRotation.Value;
            }

            Transform transform1 = transform;
            transform1.localRotation = worldRotationQuatnerion;
            transform1.localPosition = positionVector;

            if (_multiPlayersManager != null)
            {
                foreach (IConnectedPlayer player in _multiPlayersManager.allActiveAtGameStartPlayers)
                {
                    if (!_multiPlayersManager.TryGetConnectedPlayerController(player.userId, out MultiplayerConnectedPlayerFacade connectedPlayerController))
                    {
                        continue;
                    }

                    _multiplayerPositioner.transform.localPosition = connectedPlayerController.transform.position;
                    Transform avatar = connectedPlayerController.transform.Find("MultiplayerGameAvatar");
                    avatar.position = _multiplayerPositioner.transform.position;
                    avatar.rotation = _multiplayerPositioner.transform.rotation;
                }
            }

            if (_multiOutroController == null)
            {
                return;
            }

            Transform transform2 = _multiOutroController.transform;
            transform2.position = transform1.position;
            transform2.rotation = transform1.rotation;
        }
    }

    [CustomEvent(ASSIGN_PLAYER_TO_TRACK)]
    internal class AssignPlayerToTrack : ICustomEvent
    {
        private readonly IInstantiator _container;
        private readonly PlayerTransforms _playerTransforms;
        private readonly VRCenterAdjust _vrCenterAdjust;
        private readonly DeserializedData _deserializedData;
        private readonly Dictionary<PlayerTrackObject, PlayerTrack> _playerTracks = new();

        private AssignPlayerToTrack(
            IInstantiator container,
            PlayerTransforms playerTransforms,
            VRCenterAdjust vrCenterAdjust,
            [Inject(Id = ID)] DeserializedData deserializedData)
        {
            _container = container;
            _playerTransforms = playerTransforms;
            _vrCenterAdjust = vrCenterAdjust;
            _deserializedData = deserializedData;
        }

        public void Callback(CustomEventData customEventData)
        {
            if (!_deserializedData.Resolve(customEventData, out NoodlePlayerTrackEventData? noodlePlayerData))
            {
                return;
            }

            PlayerTrackObject playerTrackObject = noodlePlayerData.PlayerTrackObject;
            if (!_playerTracks.TryGetValue(playerTrackObject, out PlayerTrack? playerTrack))
            {
                _playerTracks[playerTrackObject] = playerTrack = Create(playerTrackObject);

                // unparent non-root objects so our script can set their position and afterward apply the room offset
                if (playerTrackObject != PlayerTrackObject.Root)
                {
                    Transform playerTrackTransform = playerTrack.transform;
                    playerTrackTransform.SetParent(_playerTransforms._originParentTransform.transform);

                    GameObject roomOffset = new("NoodleRoomOffset");
                    roomOffset.SetActive(false);
                    Transform roomOffsetTransform = roomOffset.transform;
                    VRCenterAdjust vrCenterAdjust = roomOffset.AddComponent<VRCenterAdjust>();
                    vrCenterAdjust._roomCenter = _vrCenterAdjust._roomCenter;
                    vrCenterAdjust._roomRotation = _vrCenterAdjust._roomRotation;
                    vrCenterAdjust._mainSettingsModel = _vrCenterAdjust._mainSettingsModel;
                    Transform target = playerTrackTransform.GetChild(0);
                    roomOffsetTransform.SetParent(playerTrackTransform);
                    roomOffset.SetActive(true);
                    target.SetParent(roomOffsetTransform, true);
                }
            }

            playerTrack.AssignTrack(noodlePlayerData.Track);
        }

        private PlayerTrack Create(PlayerTrackObject playerTrackObject)
        {
            GameObject noodleObject = new($"NoodlePlayerTrack{playerTrackObject}");
            Transform origin = noodleObject.transform;

            Transform target = playerTrackObject switch
            {
                PlayerTrackObject.Root => _playerTransforms._originTransform.parent,
                PlayerTrackObject.Head => _playerTransforms._headTransform,
                PlayerTrackObject.LeftHand => _playerTransforms._leftHandTransform,
                PlayerTrackObject.RightHand => _playerTransforms._rightHandTransform,
                _ => throw new ArgumentOutOfRangeException(nameof(playerTrackObject), playerTrackObject, null)
            };

            origin.SetParent(target.parent, false);
            target.SetParent(origin, true);

            return _container.InstantiateComponent<PlayerTrack>(noodleObject, new object[] { playerTrackObject });
        }
    }
}
