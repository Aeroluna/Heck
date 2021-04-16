namespace Chroma
{
    using IPA.Utilities;
    using NoodleExtensions;
    using NoodleExtensions.Animation;
    using UnityEngine;
    using static NoodleExtensions.Animation.AnimationHelper;
    using static NoodleExtensions.NullableExtensions;

    internal class GameObjectTrackController : MonoBehaviour
    {
        private static readonly FieldAccessor<TrackLaneRing, Vector3>.Accessor _positionOffsetAccessor = FieldAccessor<TrackLaneRing, Vector3>.GetAccessor("_positionOffset");
        private static readonly FieldAccessor<TrackLaneRing, float>.Accessor _rotZAccessor = FieldAccessor<TrackLaneRing, float>.GetAccessor("_rotZ");
        private static readonly FieldAccessor<TrackLaneRing, float>.Accessor _posZAccessor = FieldAccessor<TrackLaneRing, float>.GetAccessor("_posZ");

        private Track _track;

        private float _noteLinesDistance;

        private TrackLaneRing _trackLaneRing;

        private ParametricBoxController _parametricBoxController;

        internal static void HandleTrackData(
            GameObject gameObject,
            dynamic gameObjectData,
            IReadonlyBeatmapData beatmapData,
            float noteLinesDistance,
            TrackLaneRing trackLaneRing,
            ParametricBoxController parametricBoxController)
        {
            if (NoodleController.NoodleExtensionsActive)
            {
                Track track = NoodleExtensions.Animation.AnimationHelper.GetTrackPreload(gameObjectData, beatmapData);
                if (track != null)
                {
                    GameObjectTrackController trackController = gameObject.AddComponent<GameObjectTrackController>();
                    trackController.Init(track, noteLinesDistance, trackLaneRing, parametricBoxController);
                }
            }
        }

        internal void Init(Track track, float noteLinesDistance, TrackLaneRing trackLaneRing, ParametricBoxController parametricBoxController)
        {
            _track = track;
            _noteLinesDistance = noteLinesDistance;
            _trackLaneRing = trackLaneRing;
            _parametricBoxController = parametricBoxController;
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
                transform.localPosition = localPosition.Value * _noteLinesDistance;
            }

            if (scale.HasValue && transform.localScale != scale.Value)
            {
                transform.localScale = scale.Value;
            }

            if (_trackLaneRing != null)
            {
                if (position.HasValue || localPosition.HasValue || rotation.HasValue || localRotation.HasValue)
                {
                    if (position.HasValue || localPosition.HasValue)
                    {
                        _positionOffsetAccessor(ref _trackLaneRing) = transform.localPosition;
                        _posZAccessor(ref _trackLaneRing) = 0;
                    }

                    if (rotation.HasValue || localRotation.HasValue)
                    {
                        EnvironmentEnhancementManager.RingRotationOffsets[_trackLaneRing] = transform.localEulerAngles;
                        _rotZAccessor(ref _trackLaneRing) = 0;
                    }

                    EnvironmentEnhancementManager.SkipRingUpdate[_trackLaneRing] = true;
                }
                else
                {
                    EnvironmentEnhancementManager.SkipRingUpdate[_trackLaneRing] = false;
                }
            }

            // Handle ParametricBoxController
            if (_parametricBoxController != null)
            {
                if (position.HasValue || localPosition.HasValue)
                {
                    ParametricBoxControllerParameters.SetTransformPosition(_parametricBoxController, transform.localPosition);
                }

                if (scale.HasValue)
                {
                    ParametricBoxControllerParameters.SetTransformScale(_parametricBoxController, transform.localScale);
                }
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
