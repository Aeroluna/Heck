namespace NoodleExtensions.Animation
{
    using Heck.Animation;
    using IPA.Utilities;
    using UnityEngine;
    using static Heck.Animation.AnimationHelper;
    using static Heck.NullableExtensions;
    using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
    using static NoodleExtensions.Plugin;

    internal class PlayerTrack : MonoBehaviour
    {
        private static readonly FieldAccessor<PauseController, bool>.Accessor _pausedAccessor = FieldAccessor<PauseController, bool>.GetAccessor("_paused");

        private static PlayerTrack? _instance;
        private static Track? _track;
        private static Vector3 _startPos = Vector3.zero;
        private static Quaternion _startRot = Quaternion.identity;
        private static Quaternion _startLocalRot = Quaternion.identity;
        private static Transform? _origin;
        private static PauseController? _pauseController;

        internal static void AssignTrack(Track track)
        {
            if (_instance == null)
            {
                GameObject gameObject = GameObject.Find("LocalPlayerGameCore");
                GameObject noodleObject = new GameObject("NoodlePlayerTrack");
                _origin = noodleObject.transform;
                _origin.SetParent(gameObject.transform.parent, true);
                gameObject.transform.SetParent(_origin, true);

                _instance = noodleObject.AddComponent<PlayerTrack>();
                _pauseController = FindObjectOfType<PauseController>();
                if (_pauseController != null)
                {
                    _pauseController.didPauseEvent += _instance.OnDidPauseEvent;
                }

                _startLocalRot = _origin.localRotation;
                _startPos = _origin.localPosition;
            }

            _track = track;
        }

        private void OnDidPauseEvent()
        {
            _origin!.localRotation = _startLocalRot;
            _origin!.localPosition = _startPos;
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
            bool paused = false;
            if (_pauseController != null)
            {
                paused = _pausedAccessor(ref _pauseController);
            }

            if (!paused)
            {
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
                Quaternion? localRotation = TryGetProperty<Quaternion?>(_track, LOCALROTATION);
                if (localRotation.HasValue)
                {
                    if (LeftHandedMode)
                    {
                        MirrorQuaternionNullable(ref localRotation);
                    }

                    worldRotationQuatnerion *= localRotation!.Value;
                }

                if (_origin!.localRotation != worldRotationQuatnerion)
                {
                    _origin.localRotation = worldRotationQuatnerion;
                }

                if (_origin.localPosition != positionVector)
                {
                    _origin.localPosition = positionVector;
                }
            }
        }
    }
}
