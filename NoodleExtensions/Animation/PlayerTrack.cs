using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using Heck.Animation.Transform;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static Heck.HeckController;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.Animation
{
    internal enum PlayerTrackObject
    {
        ENTIRE_PLAYER,
        HMD,
        LEFT_HAND,
        RIGHT_HAND
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

        private Track _track = null!;
        private PauseController? _pauseController;
        private MultiplayerPlayersManager? _multiPlayersManager;
        private MultiplayerOutroAnimationController? _multiOutroController;

        private TransformController? _transformController;
        private TransformControllerFactory _transformFactory = null!;

        private GameObject _multiplayerPositioner = null!;

        internal void AssignTrack(
            Track track)
        {
            _track = track;

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
            [InjectOptional] MultiplayerOutroAnimationController? multiOutroController)
        {
            _pauseController = pauseController;
            _multiPlayersManager = multiPlayersManager;
            _multiOutroController = multiOutroController;

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

            if (pauseMenuManager != null)
            {
                pauseMenuManager.transform.SetParent(origin, false);
            }

            if (multiMenuController != null)
            {
                multiMenuController.transform.SetParent(origin, false);
            }

            _v2 = ((CustomBeatmapData)beatmapData).version2_6_0AndEarlier;
            if (!_v2)
            {
                enabled = false;
            }

            // cam2 is cringe cam2 is cringe cam2 is cringe
            _instance = this;
            _transform = origin;
        }

        private void OnDidPauseEvent()
        {
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
            Quaternion? rotation = _track.GetQuaternionProperty(OFFSET_ROTATION)?.Mirror(_leftHanded);
            Vector3? position = _track.GetVector3Property(OFFSET_POSITION)?.Mirror(_leftHanded);

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
            Quaternion? localRotation = _track.GetQuaternionProperty(LOCAL_ROTATION)?.Mirror(_leftHanded);
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

        [UsedImplicitly]
        internal class PlayerTrackFactory : IFactory<PlayerTrackObject, PlayerTrack>
        {
            private readonly IInstantiator _container;

            private PlayerTrackFactory(IInstantiator container)
            {
                _container = container;
            }

            public PlayerTrack Create(PlayerTrackObject playerTrackObject)
            {
                GameObject noodleObject = new("NoodlePlayerTrack");
                Transform origin = noodleObject.transform;

                switch (playerTrackObject)
                {
                    case PlayerTrackObject.ENTIRE_PLAYER:
                    {
                        Transform player = GameObject.Find("LocalPlayerGameCore").transform;
                        origin.SetParent(player.parent, true);
                        player.SetParent(origin, true);
                        break;
                    }

                    case PlayerTrackObject.HMD:
                        Transform hmd = GameObject.Find("VRGameCore/MainCamera").transform;
                        origin.SetParent(hmd.parent, true);
                        hmd.SetParent(origin, true);
                        break;

                    // TODO: Figure out the right name for these objects
                    // just placeholder
                    case PlayerTrackObject.LEFT_HAND:
                        Transform leftController = GameObject.Find("VRGameCore/LeftController").transform;
                        origin.SetParent(leftController.parent, true);
                        leftController.SetParent(origin, true);
                        break;
                    case PlayerTrackObject.RIGHT_HAND:
                        Transform rightController = GameObject.Find("VRGameCore/RightController").transform;
                        origin.SetParent(rightController.parent, true);
                        rightController.SetParent(origin, true);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(playerTrackObject), playerTrackObject, null);
                }

                return _container.InstantiateComponent<PlayerTrack>(noodleObject);
            }
        }
    }
}
