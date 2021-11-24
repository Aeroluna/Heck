using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using IPA.Utilities;
using UnityEngine;
using static Chroma.ChromaController;
using static Heck.Animation.AnimationHelper;
using static Heck.NullableExtensions;

namespace Chroma.Lighting.EnvironmentEnhancement
{
    internal class GameObjectTrackController : MonoBehaviour
    {
        private static readonly FieldAccessor<TrackLaneRing, Vector3>.Accessor _positionOffsetAccessor = FieldAccessor<TrackLaneRing, Vector3>.GetAccessor("_positionOffset");

        private Track _track = null!;

        private float _noteLinesDistance;

        private Transform? _parent;
        private TrackLaneRing? _trackLaneRing;
        private ParametricBoxController? _parametricBoxController;
        private BeatmapObjectsAvoidance? _beatmapObjectsAvoidance;

        private bool _handleParent;
        private bool _handleTrackLaneRing;
        private bool _handleParametricBoxController;
        private bool _handleBeatmapObjectsAvoidance;

        internal static void HandleTrackData(
            GameObject gameObject,
            Dictionary<string, object?> gameObjectData,
            CustomBeatmapData beatmapData,
            float noteLinesDistance,
            TrackLaneRing? trackLaneRing,
            ParametricBoxController? parametricBoxController,
            BeatmapObjectsAvoidance? beatmapObjectsAvoidance)
        {
            GameObjectTrackController existingTrackController = gameObject.GetComponent<GameObjectTrackController>();
            if (existingTrackController != null)
            {
                Destroy(existingTrackController);
            }

            Track? track = GetTrack(gameObjectData, beatmapData);
            if (track == null)
            {
                return;
            }

            GameObjectTrackController trackController = gameObject.AddComponent<GameObjectTrackController>();
            trackController.Init(track, noteLinesDistance, trackLaneRing, parametricBoxController, beatmapObjectsAvoidance);
        }

        internal void Init(Track track, float noteLinesDistance, TrackLaneRing? trackLaneRing, ParametricBoxController? parametricBoxController, BeatmapObjectsAvoidance? beatmapObjectsAvoidance)
        {
            _track = track;
            _noteLinesDistance = noteLinesDistance;
            _trackLaneRing = trackLaneRing;
            _parametricBoxController = parametricBoxController;
            _beatmapObjectsAvoidance = beatmapObjectsAvoidance;
            _handleTrackLaneRing = trackLaneRing != null;
            _handleParametricBoxController = parametricBoxController != null;
            _handleBeatmapObjectsAvoidance = beatmapObjectsAvoidance != null;

            track.AddGameObject(gameObject);
        }

        private void Update()
        {
            Quaternion? rotation = GetQuaternionNullable(ROTATION);
            Quaternion? localRotation = GetQuaternionNullable(LOCAL_ROTATION);
            Vector3? position = GetVectorNullable(POSITION);
            Vector3? localPosition = GetVectorNullable(LOCAL_POSITION);
            Vector3? scale = GetVectorNullable(SCALE);

            bool updateParametricBox = false;

            if (rotation.HasValue)
            {
                // Delegate positioning the object to TrackLaneRing
                Quaternion finalOffset;
                if (_handleParent)
                {
                    finalOffset = Quaternion.Inverse(transform.parent.rotation) * rotation.Value;
                }
                else
                {
                    finalOffset = rotation.Value;
                }

                if (_handleTrackLaneRing)
                {
                    EnvironmentEnhancementManager.RingRotationOffsets[_trackLaneRing!] = finalOffset;
                }
                else if (_handleBeatmapObjectsAvoidance)
                {
                    EnvironmentEnhancementManager.AvoidanceRotation[_beatmapObjectsAvoidance!] = finalOffset;
                }
                else
                {
                    transform.rotation = rotation.Value;
                }
            }

            if (localRotation.HasValue)
            {
                if (_handleTrackLaneRing)
                {
                    EnvironmentEnhancementManager.RingRotationOffsets[_trackLaneRing!] = localRotation.Value;
                }
                else if (_handleBeatmapObjectsAvoidance)
                {
                    EnvironmentEnhancementManager.AvoidanceRotation[_beatmapObjectsAvoidance!] = localRotation.Value;
                }
                else
                {
                    transform.localRotation = localRotation.Value;
                }
            }

            if (position.HasValue)
            {
                Vector3 positionValue = position.Value * _noteLinesDistance;
                Vector3 finalOffset = _handleParent ? _parent!.InverseTransformPoint(positionValue) : positionValue;

                if (_handleTrackLaneRing)
                {
                    _positionOffsetAccessor(ref _trackLaneRing!) = finalOffset;
                }
                else if (_handleBeatmapObjectsAvoidance)
                {
                    EnvironmentEnhancementManager.AvoidancePosition[_beatmapObjectsAvoidance!] = finalOffset;
                }
                else
                {
                    transform.position = positionValue;
                    updateParametricBox = true;
                }
            }

            if (localPosition.HasValue)
            {
                Vector3 localPositionValue = localPosition.Value * _noteLinesDistance;
                if (_handleTrackLaneRing)
                {
                    _positionOffsetAccessor(ref _trackLaneRing!) = localPositionValue;
                }
                else if (_handleBeatmapObjectsAvoidance)
                {
                    EnvironmentEnhancementManager.AvoidancePosition[_beatmapObjectsAvoidance!] = localPositionValue;
                }
                else
                {
                    transform.localPosition = localPositionValue;
                    updateParametricBox = true;
                }
            }

            if (scale.HasValue)
            {
                transform.localScale = scale.Value;
                updateParametricBox = true;
            }

            // Handle ParametricBoxController
            if (!updateParametricBox || !_handleParametricBoxController)
            {
                return;
            }

            ParametricBoxControllerParameters.SetTransformPosition(_parametricBoxController!, transform.localPosition);
            ParametricBoxControllerParameters.SetTransformScale(_parametricBoxController!, transform.localScale);
        }

        private void OnTransformParentChanged()
        {
            _parent = transform.parent;
            _handleParent = _parent != null;
        }

        private Vector3? GetVectorNullable(string property)
        {
            Vector3? nullable = TryGetProperty<Vector3?>(_track, property);
            if (LeftHandedMode)
            {
                MirrorVectorNullable(ref nullable);
            }

            return nullable;
        }

        private Quaternion? GetQuaternionNullable(string property)
        {
            Quaternion? nullable = TryGetProperty<Quaternion?>(_track, property);
            if (LeftHandedMode)
            {
                MirrorQuaternionNullable(ref nullable);
            }

            return nullable;
        }
    }
}
