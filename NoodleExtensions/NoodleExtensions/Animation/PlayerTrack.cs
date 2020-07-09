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
        private static Vector3 _startPos;
        private static Quaternion _startRot;
        private static Transform _origin;
        private static PauseController _pauseController;

        internal static void AssignTrack(Track track)
        {
            if (_instance == null)
            {
                GameObject gameObject = GameObject.Find("GameCore/Origin");
                _origin = gameObject.transform;
                _instance = gameObject.AddComponent<PlayerTrack>();
                _pauseController = FindObjectOfType<PauseController>();
                _startRot = _origin.localRotation;
                _startPos = _origin.localPosition;
            }

            _track = track;
        }

        private void Update()
        {
            bool paused = PauseBool(ref _pauseController);

            Quaternion rotation = _quaternionIdentity;
            if (!paused)
            {
                Quaternion? propertyRotation = TryGetProperty(_track, ROTATION);
                if (propertyRotation.HasValue)
                {
                    if (NoodleController.LeftHandedMode)
                    {
                        MirrorQuaternionNullable(ref propertyRotation);
                    }

                    rotation = propertyRotation.Value;
                }
            }

            Vector3? position = TryGetProperty(_track, POSITION);
            if (position.HasValue && !paused)
            {
                if (NoodleController.LeftHandedMode)
                {
                    MirrorVectorNullable(ref position);
                }

                _origin.localPosition = rotation * ((position.Value * NoteLinesDistance) + _startPos);
            }
            else
            {
                _origin.localPosition = rotation * _startPos;
            }

            Quaternion? localRotation = TryGetProperty(_track, LOCALROTATION);
            if (localRotation.HasValue && !paused)
            {
                if (NoodleController.LeftHandedMode)
                {
                    MirrorQuaternionNullable(ref localRotation);
                }

                _origin.localRotation = localRotation.Value * _startRot;
            }
            else
            {
                _origin.localRotation = _startRot;
            }
        }
    }
}
