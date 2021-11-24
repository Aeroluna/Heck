using System;
using Heck.Animation;
using IPA.Utilities;
using UnityEngine;
using static Heck.Animation.AnimationHelper;
using static Heck.NullableExtensions;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.Animation
{
    internal class PlayerTrack : MonoBehaviour
    {
        private static readonly FieldAccessor<PauseController, bool>.Accessor _pausedAccessor = FieldAccessor<PauseController, bool>.GetAccessor("_paused");

        private static PlayerTrack? _instance;

        private readonly Quaternion _startRot = Quaternion.identity;
        private Vector3 _startPos = Vector3.zero;
        private Quaternion _startLocalRot = Quaternion.identity;

        private Track _track = null!;
        private Transform _origin = null!;
        private PauseController _pauseController = null!;

        internal static void AssignTrack(Track track)
        {
            if (_instance == null)
            {
                GameObject gameObject = GameObject.Find("LocalPlayerGameCore");
                GameObject noodleObject = new("NoodlePlayerTrack");
                _instance = noodleObject.AddComponent<PlayerTrack>();
                Transform origin = noodleObject.transform;
                _instance._origin = origin;
                origin.SetParent(gameObject.transform.parent, true);
                gameObject.transform.SetParent(origin, true);

                PauseController pauseController = FindObjectOfType<PauseController>()
                                                  ?? throw new InvalidOperationException("Could not find PauseController.");
                pauseController.didPauseEvent += _instance.OnDidPauseEvent;
                _instance._pauseController = pauseController;

                _instance._startLocalRot = origin.localRotation;
                _instance._startPos = origin.localPosition;
            }

            _instance._track = track;
        }

        private void OnDidPauseEvent()
        {
            _origin.localRotation = _startLocalRot;
            _origin.localPosition = _startPos;
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

            Quaternion? rotation = TryGetProperty<Quaternion?>(_track, ROTATION);
            if (rotation.HasValue)
            {
                if (LeftHandedMode)
                {
                    MirrorQuaternionNullable(ref rotation);
                }
            }

            Vector3? position = TryGetProperty<Vector3?>(_track, POSITION);
            if (position.HasValue)
            {
                if (LeftHandedMode)
                {
                    MirrorVectorNullable(ref position);
                }
            }

            Quaternion worldRotationQuatnerion = _startRot;
            Vector3 positionVector = _startPos;
            if (rotation.HasValue || position.HasValue)
            {
                Quaternion finalRot = rotation ?? Quaternion.identity;
                worldRotationQuatnerion *= finalRot;
                Vector3 finalPos = position ?? Vector3.zero;
                positionVector = worldRotationQuatnerion * ((finalPos * NoteLinesDistance) + _startPos);
            }

            worldRotationQuatnerion *= _startLocalRot;
            Quaternion? localRotation = TryGetProperty<Quaternion?>(_track, LOCAL_ROTATION);
            if (localRotation.HasValue)
            {
                if (LeftHandedMode)
                {
                    MirrorQuaternionNullable(ref localRotation);
                }

                worldRotationQuatnerion *= localRotation!.Value;
            }

            _origin.localRotation = worldRotationQuatnerion;
            _origin.localPosition = positionVector;
        }
    }
}
