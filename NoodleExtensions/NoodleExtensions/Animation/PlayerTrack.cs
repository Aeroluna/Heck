namespace NoodleExtensions.Animation
{
    using IPA.Utilities;
    using UnityEngine;
    using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
    using static NoodleExtensions.Plugin;

    internal class PlayerTrack : MonoBehaviour
    {
        private static readonly FieldAccessor<PauseController, bool>.Accessor PauseBool = FieldAccessor<PauseController, bool>.GetAccessor("_paused");
        private static PlayerTrack _instance;
        private static Track _track;
        private static Vector3 _startPos;
        private static Quaternion _startRot;
        private static GameObject _origin;
        private static PauseController _pauseController;

        internal static void AssignTrack(Track track)
        {
            if (_instance == null)
            {
                _origin = GameObject.Find("GameCore/Origin");
                _instance = _origin.AddComponent<PlayerTrack>();
                _pauseController = FindObjectOfType<PauseController>();
                _startRot = _origin.transform.localRotation;
                _startPos = _origin.transform.localPosition;
            }

            _track = track;
        }

        private void Update()
        {
            bool paused = PauseBool(ref _pauseController);

            Transform transform = _origin.transform;
            object position = _track.Properties[POSITION].Value;
            if (position != null && !paused)
            {

                transform.localPosition = ((Vector3)position * NoteLinesDistance) + _startPos;
            }
            else
            {
                transform.localPosition = _startPos;
            }

            object rotation = _track.Properties[ROTATION].Value;
            if (rotation != null && !paused)
            {
                transform.localRotation = ((Quaternion)rotation) * _startRot;
            }
            else
            {
                transform.localRotation = _startRot;
            }
        }
    }
}
