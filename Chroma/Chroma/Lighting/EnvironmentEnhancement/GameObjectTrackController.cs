namespace Chroma
{
    using NoodleExtensions;
    using NoodleExtensions.Animation;
    using UnityEngine;
    using static NoodleExtensions.Animation.AnimationHelper;
    using static NoodleExtensions.NullableExtensions;

    internal class GameObjectTrackController : MonoBehaviour
    {
        private Track _track;

        private float _noteLinesDistance;

        internal static void HandleTrackData(GameObject gameObject, dynamic gameObjectData, IReadonlyBeatmapData beatmapData, float noteLinesDistance)
        {
            Track track = NoodleExtensions.Animation.AnimationHelper.GetTrackPreload(gameObjectData, beatmapData);
            if (track != null)
            {
                GameObjectTrackController trackController = gameObject.AddComponent<GameObjectTrackController>();
                trackController.Init(track, noteLinesDistance);
            }
        }

        internal void Init(Track track, float noteLinesDistance)
        {
            _track = track;
            _noteLinesDistance = noteLinesDistance;
        }

        private void Update()
        {
            Quaternion? rotation = GetQuaternionNullable("_rotation");
            Quaternion? localRotation = GetQuaternionNullable("_localRotation");
            Vector3? position = GetVectorNullable("_position");
            Vector3? localPosition = GetVectorNullable("_localPosition");
            Vector3? scale = GetVectorNullable("_scale");

            if (rotation.HasValue && transform.rotation != rotation.Value)
            {
                transform.rotation = rotation.Value;
            }

            if (localRotation.HasValue && transform.localRotation != localRotation.Value)
            {
                transform.localRotation = localRotation.Value;
            }

            if (position.HasValue && transform.position != (position.Value * _noteLinesDistance))
            {
                transform.position = position.Value * _noteLinesDistance;
            }

            if (localPosition.HasValue && transform.localPosition != localPosition.Value)
            {
                transform.localPosition = localPosition.Value;
            }

            if (scale.HasValue && transform.localScale != scale.Value)
            {
                transform.localScale = scale.Value;
            }
        }

        private Vector3? GetVectorNullable(string property)
        {
            Vector3? nullable = (Vector3?)TryGetPropertyAsObject(_track, property);
            if (nullable.HasValue)
            {
                if (NoodleController.LeftHandedMode)
                {
                    MirrorVectorNullable(ref nullable);
                }
            }

            return nullable;
        }

        private Quaternion? GetQuaternionNullable(string property)
        {
            Quaternion? nullable = (Quaternion?)TryGetPropertyAsObject(_track, property);
            if (nullable.HasValue)
            {
                if (NoodleController.LeftHandedMode)
                {
                    MirrorQuaternionNullable(ref nullable);
                }
            }

            return nullable;
        }
    }
}
