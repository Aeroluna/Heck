using Heck.Animation;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static Heck.NullableExtensions;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.Animation
{
    internal class PlayerTrack : MonoBehaviour
    {
        private static readonly FieldAccessor<PauseController, bool>.Accessor _pausedAccessor = FieldAccessor<PauseController, bool>.GetAccessor("_paused");

        private bool _leftHanded;

        private Vector3 _startPos = Vector3.zero;
        private Quaternion _startLocalRot = Quaternion.identity;

        private Track _track = null!;
        private PauseController _pauseController = null!;
        private BeatmapObjectSpawnMovementData _movementData = null!;

        internal void AssignTrack(Track track)
        {
            _track = track;
        }

        [UsedImplicitly]
        [Inject]
        private void Construct(PauseController pauseController, bool leftHanded, IBeatmapObjectSpawnController spawnController)
        {
            _pauseController = pauseController;
            pauseController.didPauseEvent += OnDidPauseEvent;
            Transform origin = transform;
            _startLocalRot = origin.localRotation;
            _startPos = origin.localPosition;
            _leftHanded = leftHanded;
            _movementData = spawnController.beatmapObjectSpawnMovementData;
        }

        private void OnDidPauseEvent()
        {
            Transform transform1 = transform;
            transform1.localRotation = _startLocalRot;
            transform1.localPosition = _startPos;
        }

        private void OnDestroy()
        {
            if (_pauseController != null)
            {
                _pauseController.didPauseEvent -= OnDidPauseEvent;
            }
        }

        private void Update()
        {
            if (_pausedAccessor(ref _pauseController))
            {
                return;
            }

            Quaternion? rotation = _track.GetProperty<Quaternion?>(ROTATION);
            if (rotation.HasValue)
            {
                if (_leftHanded)
                {
                    MirrorQuaternionNullable(ref rotation);
                }
            }

            Vector3? position = _track.GetProperty<Vector3?>(POSITION);
            if (position.HasValue)
            {
                if (_leftHanded)
                {
                    MirrorVectorNullable(ref position);
                }
            }

            Quaternion worldRotationQuatnerion = Quaternion.identity;
            Vector3 positionVector = _startPos;
            if (rotation.HasValue || position.HasValue)
            {
                Quaternion finalRot = rotation ?? Quaternion.identity;
                worldRotationQuatnerion *= finalRot;
                Vector3 finalPos = position ?? Vector3.zero;
                positionVector = worldRotationQuatnerion * ((finalPos * _movementData.noteLinesDistance) + _startPos);
            }

            worldRotationQuatnerion *= _startLocalRot;
            Quaternion? localRotation = _track.GetProperty<Quaternion?>(LOCAL_ROTATION);
            if (localRotation.HasValue)
            {
                if (_leftHanded)
                {
                    MirrorQuaternionNullable(ref localRotation);
                }

                worldRotationQuatnerion *= localRotation!.Value;
            }

            Transform transform1 = transform;
            transform1.localRotation = worldRotationQuatnerion;
            transform1.localPosition = positionVector;
        }

        [UsedImplicitly]
        internal class PlayerTrackFactory : IFactory<PlayerTrack>
        {
            private readonly DiContainer _container;

            private PlayerTrackFactory(DiContainer container)
            {
                _container = container;
            }

            public PlayerTrack Create()
            {
                Transform player = GameObject.Find("LocalPlayerGameCore").transform;
                GameObject noodleObject = new("NoodlePlayerTrack");
                Transform origin = noodleObject.transform;
                origin.SetParent(player.parent, true);
                player.SetParent(origin, true);
                return _container.InstantiateComponent<PlayerTrack>(noodleObject);
            }
        }
    }
}
