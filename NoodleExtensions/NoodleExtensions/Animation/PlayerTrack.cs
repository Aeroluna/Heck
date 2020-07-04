namespace NoodleExtensions.Animation
{
    using System;
    using IPA.Utilities;
    using UnityEngine;
    using static NoodleExtensions.Plugin;

    public class PlayerTrack : MonoBehaviour
    {
        private static readonly FieldAccessor<PauseController, bool>.Accessor PauseBool = FieldAccessor<PauseController, bool>.GetAccessor("_paused");
        private static PlayerTrack _instance;
        private static Track _track;
        private static GameObject _origin;
        private static PauseController _pauseController;

        public static void SpawnComponent(Track track)
        {
            if (!_instance)
            {
                _origin = GameObject.Find("GameCore/Origin");
                _instance = _origin.AddComponent(typeof(PlayerTrack)) as PlayerTrack;
                _pauseController = GameObject.Find("Pause").GetComponent<PauseController>();
            }

            _track = track;
        }

        void Update()
        {
            bool paused = PauseBool(ref _pauseController);
            if (_track != null)
            {
                if (_track.Properties[POSITION].Value != null && !paused)
                {
                    Vector3 pos = (Vector3)_track.Properties[POSITION].Value;
                    _origin.transform.localPosition = pos;
                } else
                {
                    _origin.transform.localPosition = Vector3.zero;
                }

                if (_track.Properties[ROTATION].Value != null && !paused)
                {
                    Quaternion rot = (Quaternion)_track.Properties[ROTATION].Value;
                    _origin.transform.localRotation = rot;
                } else
                {
                    _origin.transform.localRotation = Quaternion.identity;
                }
            }
        }
    }
}
