namespace Chroma
{
    using System.Collections.Generic;
    using Heck.Animation;
    using IPA.Utilities;
    using UnityEngine;
    using static Heck.Animation.AnimationHelper;
    using static Heck.NullableExtensions;

    internal class GameObjectTrackController : MonoBehaviour
    {
        private static readonly FieldAccessor<TrackLaneRing, Vector3>.Accessor _positionOffsetAccessor = FieldAccessor<TrackLaneRing, Vector3>.GetAccessor("_positionOffset");

        private Track _track;

        private float _noteLinesDistance;

        private TrackLaneRing _trackLaneRing;

        private ParametricBoxController _parametricBoxController;

        private BeatmapObjectsAvoidance _beatmapObjectsAvoidance;

        internal static void HandleTrackData(
            GameObject gameObject,
            Dictionary<string, object> gameObjectData,
            IReadonlyBeatmapData beatmapData,
            float noteLinesDistance,
            TrackLaneRing trackLaneRing,
            ParametricBoxController parametricBoxController,
            BeatmapObjectsAvoidance beatmapObjectsAvoidance)
        {
            Track track = GetTrack(gameObjectData, beatmapData);
            if (track != null)
            {
                GameObjectTrackController trackController = gameObject.AddComponent<GameObjectTrackController>();
                trackController.Init(track, noteLinesDistance, trackLaneRing, parametricBoxController, beatmapObjectsAvoidance);
            }
        }

        internal void Init(Track track, float noteLinesDistance, TrackLaneRing trackLaneRing, ParametricBoxController parametricBoxController, BeatmapObjectsAvoidance beatmapObjectsAvoidance)
        {
            _track = track;
            _noteLinesDistance = noteLinesDistance;
            _trackLaneRing = trackLaneRing;
            _parametricBoxController = parametricBoxController;
            _beatmapObjectsAvoidance = beatmapObjectsAvoidance;
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
                Quaternion finalOffset;
                if (transform.parent != null)
                {
                    finalOffset = Quaternion.Inverse(transform.parent.rotation) * rotation.Value;
                }
                else
                {
                    finalOffset = rotation.Value;
                }

                if (_trackLaneRing != null)
                {
                    EnvironmentEnhancementManager.RingRotationOffsets[_trackLaneRing] = finalOffset;
                }
                else if (_beatmapObjectsAvoidance != null)
                {
                    EnvironmentEnhancementManager.AvoidanceRotation[_beatmapObjectsAvoidance] = finalOffset;
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
                else if (_beatmapObjectsAvoidance != null)
                {
                    EnvironmentEnhancementManager.AvoidanceRotation[_beatmapObjectsAvoidance] = localRotation.Value;
                }
                else
                {
                    transform.localRotation = localRotation.Value;
                }
            }

            if (position.HasValue && transform.position != (position.Value * _noteLinesDistance))
            {
                Vector3 positionValue = position.Value * _noteLinesDistance;
                Vector3 finalOffset;
                if (transform.parent != null)
                {
                    finalOffset = transform.parent.InverseTransformPoint(positionValue);
                }
                else
                {
                    finalOffset = positionValue;
                }

                if (_trackLaneRing != null)
                {
                    _positionOffsetAccessor(ref _trackLaneRing) = finalOffset;
                }
                else if (_beatmapObjectsAvoidance != null)
                {
                    EnvironmentEnhancementManager.AvoidancePosition[_beatmapObjectsAvoidance] = finalOffset;
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
                else if (_beatmapObjectsAvoidance != null)
                {
                    EnvironmentEnhancementManager.AvoidancePosition[_beatmapObjectsAvoidance] = localPositionValue;
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
            Vector3? nullable = (Vector3?)TryGetProperty(_track, property);
            if (nullable.HasValue)
            {
                if (LeftHandedMode)
                {
                    MirrorVectorNullable(ref nullable);
                }
            }

            return nullable;
        }

        private Quaternion? GetQuaternionNullable(string property)
        {
            Quaternion? nullable = (Quaternion?)TryGetProperty(_track, property);
            if (nullable.HasValue)
            {
                if (LeftHandedMode)
                {
                    MirrorQuaternionNullable(ref nullable);
                }
            }

            return nullable;
        }
    }
}
