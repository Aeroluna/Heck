using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static Chroma.ChromaController;
using static Heck.HeckController;
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
        private TubeBloomPrePassLight? _tubeBloomPrePassLight;
        private Material? _material;

        private bool _handleParent;
        private bool _handleTrackLaneRing;
        private bool _handleParametricBoxController;
        private bool _handleBeatmapObjectsAvoidance;
        private bool _handleTubeBloomPrePassLight;
        private bool _handleMaterial;
        private bool _v2;

        private bool _leftHanded;
        private EnvironmentEnhancementManager _environmentManager = null!;
        private ParametricBoxControllerParameters _parametricBoxControllerParameters = null!;

        internal static GameObjectTrackController? HandleTrackData(
            Factory factory,
            GameObject gameObject,
            CustomData gameObjectData,
            float noteLinesDistance,
            TrackLaneRing? trackLaneRing,
            TubeBloomPrePassLight? tubeBloomPrePassLight,
            Material? material,
            ParametricBoxController? parametricBoxController,
            BeatmapObjectsAvoidance? beatmapObjectsAvoidance,
            Dictionary<string, Track> tracks,
            bool v2)
        {
            GameObjectTrackController existingTrackController = gameObject.GetComponent<GameObjectTrackController>();
            if (existingTrackController != null)
            {
                Destroy(existingTrackController);
            }

            // TODO: stop being lazy and deserialize the tracks properly
            Track? track = gameObjectData.GetNullableTrack(tracks, v2);
            if (track == null)
            {
                return null;
            }

            // this is NOT the correct way to do this, but fuck you im lazy.
            GameObjectTrackController trackController = factory.Create(gameObject);
            trackController.Init(track, noteLinesDistance, trackLaneRing, tubeBloomPrePassLight, material, parametricBoxController, beatmapObjectsAvoidance, v2);
            return trackController;
        }

        internal void Init(
            Track track,
            float noteLinesDistance,
            TrackLaneRing? trackLaneRing,
            TubeBloomPrePassLight? tubeBloomPrePassLight,
            Material? material,
            ParametricBoxController? parametricBoxController,
            BeatmapObjectsAvoidance? beatmapObjectsAvoidance,
            bool v2)
        {
            _track = track;
            _noteLinesDistance = noteLinesDistance;
            _trackLaneRing = trackLaneRing;
            _tubeBloomPrePassLight = tubeBloomPrePassLight;
            _material = material;
            _parametricBoxController = parametricBoxController;
            _beatmapObjectsAvoidance = beatmapObjectsAvoidance;
            _handleTrackLaneRing = trackLaneRing != null;
            _handleTubeBloomPrePassLight = tubeBloomPrePassLight != null;
            _handleMaterial = material != null;
            _handleParametricBoxController = parametricBoxController != null;
            _handleBeatmapObjectsAvoidance = beatmapObjectsAvoidance != null;
            _v2 = v2;

            track.AddGameObject(gameObject);
        }

        [UsedImplicitly]
        [Inject]
        private void Construct(
            [Inject(Id = LEFT_HANDED_ID)] bool leftHanded,
            EnvironmentEnhancementManager environmentManager,
            ParametricBoxControllerParameters parametricBoxControllerParameters)
        {
            _leftHanded = leftHanded;
            _environmentManager = environmentManager;
            _parametricBoxControllerParameters = parametricBoxControllerParameters;
        }

        private void Update()
        {
            Quaternion? rotation = GetQuaternionNullable(ROTATION);
            Quaternion? localRotation = GetQuaternionNullable(LOCAL_ROTATION);
            Vector3? position = GetVectorNullable(POSITION);
            Vector3? localPosition = GetVectorNullable(LOCAL_POSITION);
            Vector3? scale = GetVectorNullable(SCALE);
            Color? color = GetColorNullable(COLOR);

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
                    _environmentManager.RingRotationOffsets[_trackLaneRing!] = finalOffset;
                }
                else if (_handleBeatmapObjectsAvoidance)
                {
                    _environmentManager.AvoidanceRotation[_beatmapObjectsAvoidance!] = finalOffset;
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
                    _environmentManager.RingRotationOffsets[_trackLaneRing!] = localRotation.Value;
                }
                else if (_handleBeatmapObjectsAvoidance)
                {
                    _environmentManager.AvoidanceRotation[_beatmapObjectsAvoidance!] = localRotation.Value;
                }
                else
                {
                    transform.localRotation = localRotation.Value;
                }
            }

            if (position.HasValue)
            {
                Vector3 positionValue = position.Value;
                if (_v2)
                {
                    positionValue *= _noteLinesDistance;
                }

                Vector3 finalOffset = _handleParent ? _parent!.InverseTransformPoint(positionValue) : positionValue;

                if (_handleTrackLaneRing)
                {
                    _positionOffsetAccessor(ref _trackLaneRing!) = finalOffset;
                }
                else if (_handleBeatmapObjectsAvoidance)
                {
                    _environmentManager.AvoidancePosition[_beatmapObjectsAvoidance!] = finalOffset;
                }
                else
                {
                    transform.position = positionValue;
                    updateParametricBox = true;
                }
            }

            if (localPosition.HasValue)
            {
                Vector3 localPositionValue = localPosition.Value;
                if (_v2)
                {
                    localPositionValue *= _noteLinesDistance;
                }

                if (_handleTrackLaneRing)
                {
                    _positionOffsetAccessor(ref _trackLaneRing!) = localPositionValue;
                }
                else if (_handleBeatmapObjectsAvoidance)
                {
                    _environmentManager.AvoidancePosition[_beatmapObjectsAvoidance!] = localPositionValue;
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

            if (color.HasValue)
            {
                if (_handleTubeBloomPrePassLight)
                {
                    _tubeBloomPrePassLight!.color = color.Value;
                }

                if (_handleMaterial)
                {
                    _material!.color = color.Value;
                }
            }

            // Handle ParametricBoxController
            if (!updateParametricBox || !_handleParametricBoxController)
            {
                return;
            }

            _parametricBoxControllerParameters.SetTransformPosition(_parametricBoxController!, transform.localPosition);
            _parametricBoxControllerParameters.SetTransformScale(_parametricBoxController!, transform.localScale);
        }

        private void OnTransformParentChanged()
        {
            _parent = transform.parent;
            _handleParent = _parent != null;
        }

        private Vector3? GetVectorNullable(string property)
        {
            Vector3? nullable = _track.GetProperty<Vector3?>(property);
            if (_leftHanded)
            {
                MirrorVectorNullable(ref nullable);
            }

            return nullable;
        }

        private Color? GetColorNullable(string property)
        {
            Color? nullable = _track.GetProperty<Vector4?>(property);


            return nullable;
        }

        private Quaternion? GetQuaternionNullable(string property)
        {
            Quaternion? nullable = _track.GetProperty<Quaternion?>(property);
            if (_leftHanded)
            {
                MirrorQuaternionNullable(ref nullable);
            }

            return nullable;
        }

        [UsedImplicitly]
        internal class Factory : PlaceholderFactory<GameObject, GameObjectTrackController>
        {
        }

        [UsedImplicitly]
        internal class GameObjectTrackControllerFactory : IFactory<GameObject, GameObjectTrackController>
        {
            private readonly IInstantiator _container;

            private GameObjectTrackControllerFactory(IInstantiator container)
            {
                _container = container;
            }

            public GameObjectTrackController Create(GameObject gameObject)
            {
                return _container.InstantiateComponent<GameObjectTrackController>(gameObject);
            }
        }
    }
}
