namespace NoodleExtensions.Animation
{
using System;
using UnityEngine;
using static NoodleExtensions.Plugin;

public class PlayerTrack : MonoBehaviour
    {
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

            }

            _track = track;
        }

        void Update()
        {
            if (_track != null)
            {
                if (_track.Properties[POSITION].Value != null)
                {
                    Vector3 pos = (Vector3)_track.Properties[POSITION].Value;
                    _origin.transform.localPosition = pos;
                } else
                {
                    _origin.transform.localPosition = Vector3.zero;
                }

                if (_track.Properties[ROTATION].Value != null)
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
