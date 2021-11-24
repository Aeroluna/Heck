using System;
using System.Collections.Generic;
using Heck.Animation;
using UnityEngine;
using static Heck.Animation.AnimationHelper;
using static Heck.NullableExtensions;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.Animation
{
    internal class ParentObject : MonoBehaviour
    {
        private Track _track = null!;
        private Transform _origin = null!;
        private bool _worldPositionStays;
        private Vector3 _startPos = Vector3.zero;
        private Quaternion _startRot = Quaternion.identity;
        private Quaternion _startLocalRot = Quaternion.identity;
        private Vector3 _startScale = Vector3.one;

        internal static ParentController? Controller { get; private set; }

        internal HashSet<Track> ChildrenTracks { get; } = new();

        internal static void AssignTrack(List<Track> tracks, Track parentTrack, bool worldPositionStays, Vector3? startPos, Quaternion? startRot, Quaternion? startLocalRot, Vector3? startScale)
        {
            if (tracks.Contains(parentTrack))
            {
                throw new InvalidOperationException("How could a track contain itself?");
            }

            if (Controller == null)
            {
                GameObject gameObject = new("ParentController");
                Controller = gameObject.AddComponent<ParentController>();
            }

            GameObject parentGameObject = new("ParentObject");
            ParentObject instance = parentGameObject.AddComponent<ParentObject>();
            instance._origin = parentGameObject.transform;
            instance._track = parentTrack;
            instance._worldPositionStays = worldPositionStays;

            Transform transform = instance.transform;
            if (startPos.HasValue)
            {
                instance._startPos = startPos.Value;
                transform.localPosition = instance._startPos * NoteLinesDistance;
            }

            if (startRot.HasValue)
            {
                instance._startRot = startRot.Value;
                instance._startLocalRot = instance._startRot;
                transform.localPosition = instance._startRot * transform.localPosition;
                transform.localRotation = instance._startRot;
            }

            if (startLocalRot.HasValue)
            {
                instance._startLocalRot = instance._startRot * startLocalRot.Value;
                transform.localRotation *= instance._startLocalRot;
            }

            if (startScale.HasValue)
            {
                instance._startScale = startScale.Value;
                transform.localScale = instance._startScale;
            }

            parentTrack.AddGameObject(parentGameObject);

            foreach (Track track in tracks)
            {
                foreach (ParentObject parentObject in Controller.ParentObjects)
                {
                    track.OnGameObjectAdded -= parentObject.OnTrackGameObjectAdded;
                    track.OnGameObjectRemoved -= OnTrackGameObjectRemoved;
                    parentObject.ChildrenTracks.Remove(track);
                }

                foreach (GameObject gameObject in track.GameObjects)
                {
                    instance.ParentToObject(gameObject.transform);
                }

                instance.ChildrenTracks.Add(track);

                track.OnGameObjectAdded += instance.OnTrackGameObjectAdded;
                track.OnGameObjectRemoved += OnTrackGameObjectRemoved;
            }

            Controller.ParentObjects.Add(instance);
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
            childTransform.SetParent(_origin.transform, _worldPositionStays);
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
            Quaternion? rotation = TryGetProperty<Quaternion?>(_track, ROTATION);
            if (rotation.HasValue)
            {
                if (LeftHandedMode)
                {
                    MirrorQuaternionNullable(ref rotation);
                }
            }

            Vector3? position = TryGetProperty<Vector3?>(_track, POSITION);
            if (position.HasValue)
            {
                if (LeftHandedMode)
                {
                    MirrorVectorNullable(ref position);
                }
            }

            Quaternion worldRotationQuatnerion = _startRot;
            Vector3 positionVector = worldRotationQuatnerion * (_startPos * NoteLinesDistance);
            if (rotation.HasValue || position.HasValue)
            {
                Quaternion rotationOffset = rotation ?? Quaternion.identity;
                worldRotationQuatnerion *= rotationOffset;
                Vector3 positionOffset = position ?? Vector3.zero;
                positionVector = worldRotationQuatnerion * ((positionOffset + _startPos) * NoteLinesDistance);
            }

            worldRotationQuatnerion *= _startLocalRot;
            Quaternion? localRotation = TryGetProperty<Quaternion?>(_track, LOCAL_ROTATION);
            if (localRotation.HasValue)
            {
                if (LeftHandedMode)
                {
                    MirrorQuaternionNullable(ref localRotation);
                }

                worldRotationQuatnerion *= localRotation!.Value;
            }

            Vector3 scaleVector = _startScale;
            Vector3? scale = TryGetProperty<Vector3?>(_track, SCALE);
            if (scale.HasValue)
            {
                scaleVector = Vector3.Scale(_startScale, scale.Value);
            }

            _origin.localRotation = worldRotationQuatnerion;
            _origin.localPosition = positionVector;
            _origin.localScale = scaleVector;
        }
    }

    internal class ParentController : MonoBehaviour
    {
        internal HashSet<ParentObject> ParentObjects { get; } = new();

        private void OnDestroy()
        {
            ParentObjects.Clear();
        }
    }
}
