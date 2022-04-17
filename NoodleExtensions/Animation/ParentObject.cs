using System;
using System.Collections.Generic;
using Heck.Animation;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static Heck.HeckController;
using static Heck.NullableExtensions;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.Animation
{
    internal class ParentObject : MonoBehaviour
    {
        private Track _track = null!;
        private bool _worldPositionStays;
        private bool _leftHanded;
        private Vector3 _startPos = Vector3.zero;
        private Quaternion _startRot = Quaternion.identity;
        private Quaternion _startLocalRot = Quaternion.identity;
        private Vector3 _startScale = Vector3.one;

        private BeatmapObjectSpawnMovementData _movementData = null!;

        internal HashSet<Track> ChildrenTracks { get; } = new();

        internal void Init(
            List<Track> tracks,
            Track parentTrack,
            bool worldPositionStays,
            Vector3? startPos,
            Quaternion? startRot,
            Quaternion? startLocalRot,
            Vector3? startScale,
            bool leftHanded,
            BeatmapObjectSpawnMovementData movementData,
            HashSet<ParentObject> parentObjects)
        {
            if (tracks.Contains(parentTrack))
            {
                throw new InvalidOperationException("How could a track contain itself?");
            }

            Transform transform1 = transform;
            _track = parentTrack;
            _worldPositionStays = worldPositionStays;
            _leftHanded = leftHanded;
            _movementData = movementData;

            if (startPos.HasValue)
            {
                _startPos = startPos.Value;
                transform1.localPosition = _startPos * movementData.noteLinesDistance;
            }

            if (startRot.HasValue)
            {
                _startRot = startRot.Value;
                _startLocalRot = _startRot;
                transform1.localPosition = _startRot * transform1.localPosition;
                transform1.localRotation = _startRot;
            }

            if (startLocalRot.HasValue)
            {
                _startLocalRot = _startRot * startLocalRot.Value;
                transform1.localRotation *= _startLocalRot;
            }

            if (startScale.HasValue)
            {
                _startScale = startScale.Value;
                transform1.localScale = _startScale;
            }

            parentTrack.AddGameObject(gameObject);

            foreach (Track track in tracks)
            {
                foreach (ParentObject parentObject in parentObjects)
                {
                    track.OnGameObjectAdded -= parentObject.OnTrackGameObjectAdded;
                    track.OnGameObjectRemoved -= OnTrackGameObjectRemoved;
                    parentObject.ChildrenTracks.Remove(track);
                }

                foreach (GameObject go in track.GameObjects)
                {
                    ParentToObject(go.transform);
                }

                ChildrenTracks.Add(track);

                track.OnGameObjectAdded += OnTrackGameObjectAdded;
                track.OnGameObjectRemoved += OnTrackGameObjectRemoved;
            }

            parentObjects.Add(this);
        }

        private static void ResetTransformParent(Transform transform)
        {
            transform.SetParent(null, false);
        }

        private static void OnTrackGameObjectRemoved(GameObject trackGameObject)
        {
            ResetTransformParent(trackGameObject.transform);
        }

        private void OnTrackGameObjectAdded(GameObject trackGameObject)
        {
            ParentToObject(trackGameObject.transform);
        }

        private void ParentToObject(Transform childTransform)
        {
            childTransform.SetParent(transform, _worldPositionStays);
        }

        private void OnDestroy()
        {
            foreach (Track track in ChildrenTracks)
            {
                track.OnGameObjectAdded -= OnTrackGameObjectAdded;
                track.OnGameObjectRemoved -= OnTrackGameObjectRemoved;
            }
        }

        private void Update()
        {
            Quaternion? rotation = _track.GetProperty<Quaternion?>(WORLD_ROTATION);
            if (rotation.HasValue)
            {
                if (_leftHanded)
                {
                    MirrorQuaternionNullable(ref rotation);
                }
            }

            // TODO: wtf clean up this mess
            Vector3? position = _track.GetProperty<Vector3?>(OFFSET_POSITION);
            if (position.HasValue)
            {
                if (_leftHanded)
                {
                    MirrorVectorNullable(ref position);
                }
            }

            Quaternion worldRotationQuatnerion = _startRot;
            Vector3 positionVector = worldRotationQuatnerion * (_startPos * _movementData.noteLinesDistance);
            if (rotation.HasValue || position.HasValue)
            {
                Quaternion rotationOffset = rotation ?? Quaternion.identity;
                worldRotationQuatnerion *= rotationOffset;
                Vector3 positionOffset = position ?? Vector3.zero;
                positionVector = worldRotationQuatnerion * ((positionOffset + _startPos) * _movementData.noteLinesDistance);
            }

            worldRotationQuatnerion *= _startLocalRot;
            Quaternion? localRotation = _track.GetProperty<Quaternion?>(LOCAL_ROTATION);
            if (localRotation.HasValue)
            {
                if (_leftHanded)
                {
                    MirrorQuaternionNullable(ref localRotation);
                }

                worldRotationQuatnerion *= localRotation!.Value;
            }

            Vector3 scaleVector = _startScale;
            Vector3? scale = _track.GetProperty<Vector3?>(SCALE);
            if (scale.HasValue)
            {
                scaleVector = Vector3.Scale(_startScale, scale.Value);
            }

            Transform transform1 = transform;
            transform1.localRotation = worldRotationQuatnerion;
            transform1.localPosition = positionVector;
            transform1.localScale = scaleVector;
        }
    }

    [UsedImplicitly]
    internal class ParentController
    {
        private readonly bool _leftHanded;
        private readonly HashSet<ParentObject> _parentObjects = new();
        private readonly BeatmapObjectSpawnMovementData _movementData;

        internal ParentController([Inject(Id = LEFT_HANDED_ID)] bool leftHanded, IBeatmapObjectSpawnController spawnController)
        {
            _leftHanded = leftHanded;
            _movementData = spawnController.beatmapObjectSpawnMovementData;
        }

        internal void Create(
            List<Track> tracks,
            Track parentTrack,
            bool worldPositionStays,
            Vector3? startPos,
            Quaternion? startRot,
            Quaternion? startLocalRot,
            Vector3? startScale)
        {
            GameObject parentGameObject = new("ParentObject");
            ParentObject instance = parentGameObject.AddComponent<ParentObject>();
            instance.Init(
                tracks,
                parentTrack,
                worldPositionStays,
                startPos,
                startRot,
                startLocalRot,
                startScale,
                _leftHanded,
                _movementData,
                _parentObjects);
        }
    }
}
