namespace NoodleExtensions.Animation
{
    using IPA.Utilities;
    using UnityEngine;
    using static NoodleExtensions.Animation.AnimationHelper;
    using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
    using static NoodleExtensions.NullableExtensions;
    using static NoodleExtensions.Plugin;

    internal class PlayerTrack : MonoBehaviour
    {
        private static readonly FieldAccessor<PauseController, bool>.Accessor PauseBool = FieldAccessor<PauseController, bool>.GetAccessor("_paused");
        private static PlayerTrack _instance;
        private static Track _track;
        private static Vector3 _startPos = _vectorZero;
        private static Quaternion _startRot = _quaternionIdentity;
        private static Quaternion _startLocalRot = _quaternionIdentity;
        private static Transform _origin;
        private static PauseController _pauseController;

        internal static void AssignTrack(Track track)
        {
            if (_instance == null)
            {
                GameObject gameObject = GameObject.Find("LocalPlayerGameCore/Origin");
                _origin = gameObject.transform;
                _instance = gameObject.AddComponent<PlayerTrack>();
                _pauseController = FindObjectOfType<PauseController>();
                _startLocalRot = _origin.localRotation;
                _startPos = _origin.localPosition;
            }

            _track = track;
        }

        private void Update()
        {
            bool paused = false;
            if (_pauseController != null)
                paused = PauseBool(ref _pauseController);

            Quaternion? rotation = TryGetProperty(_track, ROTATION);
            if (rotation.HasValue)
            {
                if (NoodleController.LeftHandedMode)
                {
                    MirrorQuaternionNullable(ref rotation);
                }
            }

            Vector3? position = TryGetProperty(_track, POSITION);
            if (position.HasValue)
            {
                if (NoodleController.LeftHandedMode)
                {
                    MirrorVectorNullable(ref position);
                }
            }

            Quaternion worldRotationQuatnerion = _startRot;
            Vector3 positionVector = _startPos;
            if ((rotation.HasValue || position.HasValue) && !paused)
            {
                Quaternion finalRot = rotation ?? _quaternionIdentity;
                worldRotationQuatnerion *= finalRot;
                Vector3 finalPos = position ?? _vectorZero;
                positionVector = worldRotationQuatnerion * ((finalPos * NoteLinesDistance) + _startPos);
            }

            worldRotationQuatnerion *= _startLocalRot;
            Quaternion? localRotation = TryGetProperty(_track, LOCALROTATION);
            if (localRotation.HasValue && !paused)
            {
                if (NoodleController.LeftHandedMode)
                {
                    MirrorQuaternionNullable(ref localRotation);
                }

                worldRotationQuatnerion *= localRotation.Value;
            }

            if (_origin.localRotation != worldRotationQuatnerion)
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
