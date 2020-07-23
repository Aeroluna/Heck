namespace NoodleExtensions.Animation
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;
    using static NoodleExtensions.Animation.AnimationHelper;
    using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
    using static NoodleExtensions.NullableExtensions;
    using static NoodleExtensions.Plugin;

    internal class ParentObject : MonoBehaviour
    {
        private Track _track;
        private Transform _origin;
        private Vector3 _startPos = _vectorZero;
        private Quaternion _startRot = _quaternionIdentity;
        private Quaternion _startLocalRot = _quaternionIdentity;
        private Vector3 _startScale = Vector3.one;

        internal static ParentController Controller { get; private set; }

        internal HashSet<Track> ChildrenTracks { get; } = new HashSet<Track>();

        internal static void ResetTransformParent(Transform transform)
        {
            transform.SetParent(null, false);
        }

        internal static void AssignTrack(IEnumerable<Track> tracks, Track parentTrack, Vector3? startPos, Quaternion? startRot, Quaternion? startLocalRot, Vector3? startScale)
        {
            if (Controller == null)
            {
                GameObject gameObject = new GameObject("ParentController");
                Controller = gameObject.AddComponent<ParentController>();
            }

            GameObject parentGameObject = new GameObject("ParentObject");
            ParentObject instance = parentGameObject.AddComponent<ParentObject>();
            instance._origin = parentGameObject.transform;
            instance._track = parentTrack;

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
                transform.localRotation = transform.localRotation * instance._startLocalRot;
            }

            if (startScale.HasValue)
            {
                instance._startScale = startScale.Value;
                transform.localScale = instance._startScale;
            }

            foreach (ParentObject parentObject in Controller.ParentObjects)
            {
                if (parentObject.ChildrenTracks.Contains(parentTrack))
                {
                    parentObject.ParentToObject(transform);
                }
                else
                {
                    ResetTransformParent(transform);
                }
            }

            foreach (Track track in tracks)
            {
                foreach (ParentObject parentObject in Controller.ParentObjects)
                {
                    parentObject.ChildrenTracks.Remove(track);

                    if (parentObject._track == track)
                    {
                        instance.ParentToObject(parentObject.transform);
                    }
                }

                foreach (ObstacleController obstacleController in ObstaclePool.activeItems)
                {
                    if (obstacleController.obstacleData is CustomObstacleData customObstacleData)
                    {
                        Track obstacleTrack = GetTrack(customObstacleData.customData);
                        if (obstacleTrack == track)
                        {
                            instance.ParentToObject(obstacleController.transform);
                        }
                    }
                }

                instance.ChildrenTracks.Add(track);
            }

            Controller.ParentObjects.Add(instance);
        }

        internal void ParentToObject(Transform transform)
        {
            transform.SetParent(_origin.transform, false);
        }

        private void Update()
        {
            Quaternion? rotation = TryGetProperty(_track, ROTATION);
            if (rotation.HasValue)
            {
                if (NoodleController.LeftHandedMode)
                {
                    MirrorQuaternionNullable(ref rotation);
                }
            }

            Vector3? position = TryGetProperty(_track, POSITION);
            if (position.HasValue)
            {
                if (NoodleController.LeftHandedMode)
                {
                    MirrorVectorNullable(ref position);
                }
            }

            Quaternion worldRotationQuatnerion = _startRot;
            Vector3 positionVector = worldRotationQuatnerion * (_startPos * NoteLinesDistance);
            if (rotation.HasValue || position.HasValue)
            {
                Quaternion rotationOffset = rotation.HasValue ? rotation.Value : _quaternionIdentity;
                worldRotationQuatnerion *= rotationOffset;
                Vector3 positionOffset = position.HasValue ? position.Value : _vectorZero;
                positionVector = worldRotationQuatnerion * ((positionOffset + _startPos) * NoteLinesDistance);
            }

            worldRotationQuatnerion *= _startLocalRot;
            Quaternion? localRotation = TryGetProperty(_track, LOCALROTATION);
            if (localRotation.HasValue)
            {
                if (NoodleController.LeftHandedMode)
                {
                    MirrorQuaternionNullable(ref localRotation);
                }

                worldRotationQuatnerion *= localRotation.Value;
            }

            Vector3 scaleVector = _startScale;
            Vector3? scale = TryGetProperty(_track, SCALE);
            if (scale.HasValue)
            {
                scaleVector = Vector3.Scale(_startScale, scale.Value);
            }

            if (_origin.localRotation != worldRotationQuatnerion)
            {
                _origin.localRotation = worldRotationQuatnerion;
            }

            if (_origin.localPosition != positionVector)
            {
                _origin.localPosition = positionVector;
            }

            if (_origin.localScale != scaleVector)
            {
                _origin.localScale = scaleVector;
            }
        }
    }

    internal class ParentController : MonoBehaviour
    {
        internal HashSet<ParentObject> ParentObjects { get; } = new HashSet<ParentObject>();

        internal ParentObject GetParentObjectTrack(Track track)
        {
            IEnumerable<ParentObject> filteredParents = ParentObjects.Where(n => n.ChildrenTracks.Contains(track));
            if (filteredParents != null)
            {
                return filteredParents.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        private void OnDestroy()
        {
            ParentObjects.Clear();
        }
    }
}
