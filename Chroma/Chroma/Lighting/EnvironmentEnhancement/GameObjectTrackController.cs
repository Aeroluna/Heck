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
                // Delegate positioning the object to TrackLaneRing
                if (_trackLaneRing != null)
                {
                    Quaternion finalOffset;
                    if (transform.parent != null)
                    {
                        finalOffset = Quaternion.Inverse(transform.parent.rotation) * rotation.Value;
                    }
                    else
                    {
                        finalOffset = rotation.Value;
                    }

                    EnvironmentEnhancementManager.RingRotationOffsets[_trackLaneRing] = finalOffset;
                }
                else
                {
                    transform.rotation = rotation.Value;
                }
            }

            if (localRotation.HasValue && transform.localRotation != localRotation.Value)
            {
                if (_trackLaneRing != null)
                {
                    EnvironmentEnhancementManager.RingRotationOffsets[_trackLaneRing] = localRotation.Value;
                }
                else
                {
                    transform.localRotation = localRotation.Value;
                }
            }

            if (position.HasValue && transform.position != (position.Value * _noteLinesDistance))
            {
                Vector3 positionValue = position.Value * _noteLinesDistance;
                if (_trackLaneRing != null)
                {
                    Vector3 finalOffset;
                    if (transform.parent != null)
                    {
                        finalOffset = transform.parent.InverseTransformPoint(positionValue);
                    }
                    else
                    {
                        finalOffset = positionValue;
                    }

                    _positionOffsetAccessor(ref _trackLaneRing) = finalOffset;
                }
                else
                {
                    transform.position = positionValue;
                }
            }

            if (localPosition.HasValue && transform.localPosition != localPosition.Value)
            {
                Vector3 localPositionValue = localPosition.Value * _noteLinesDistance;
                if (_trackLaneRing != null)
                {
                    _positionOffsetAccessor(ref _trackLaneRing) = localPositionValue;
                }
                else
                {
                    transform.localPosition = localPositionValue;
                }
            }

            if (scale.HasValue && transform.localScale != scale.Value)
            {
                transform.localScale = scale.Value;
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
