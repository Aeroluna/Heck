using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using JetBrains.Annotations;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;
using static Heck.HeckController;
using static Heck.NullableExtensions;
using Object = UnityEngine.Object;

namespace Heck.Animation.Transform
{
    // General controller script for GameObjects
    public class TransformController : MonoBehaviour
    {
        private bool _v2;
        private bool _leftHanded;
        private List<Track>? _track;

        public event Action? RotationUpdated;

        public event Action? PositionUpdated;

        public event Action? ScaleUpdated;

        [Inject]
        [UsedImplicitly]
        private void Construct(
            [Inject(Id = LEFT_HANDED_ID)] bool leftHanded,
            IReadonlyBeatmapData beatmapData,
            List<Track> track)
        {
            _leftHanded = leftHanded;
            _v2 = beatmapData is CustomBeatmapData { version2_6_0AndEarlier: true };
            _track = track;
        }

        private void OnEnable()
        {
            UpdatePos();
        }

        private void OnTransformParentChanged()
        {
            UpdatePos();
        }

        private void Update()
        {
            if (_track != null && _track.Any(n => n.UpdatedThisFrame))
            {
                UpdatePos();
            }
        }

        private void UpdatePos()
        {
            if (_track == null)
            {
                return;
            }

            Vector3? scale;
            Quaternion? rotation;
            Quaternion? localRotation;
            Vector3? position;
            Vector3? localPosition;

            if (_track.Count > 1)
            {
                scale = MultVectorNullables(_track.Select(n => n.GetProperty<Vector3>(SCALE)));
                rotation = MultQuaternionNullables(_track.Select(n => n.GetProperty<Quaternion>(ROTATION)));
                localRotation = MultQuaternionNullables(_track.Select(n => n.GetProperty<Quaternion>(LOCAL_ROTATION)));
                position = SumVectorNullables(_track.Select(n => n.GetProperty<Vector3>(POSITION)));
                localPosition = SumVectorNullables(_track.Select(n => n.GetProperty<Vector3>(LOCAL_POSITION)));
            }
            else
            {
                Track track = _track.First();
                scale = track.GetProperty<Vector3>(SCALE);
                rotation = track.GetProperty<Quaternion>(ROTATION);
                localRotation = track.GetProperty<Quaternion>(LOCAL_ROTATION);
                position = track.GetProperty<Vector3>(POSITION);
                localPosition = track.GetProperty<Vector3>(LOCAL_POSITION);
            }

            if (_leftHanded)
            {
                rotation = rotation?.Mirror();
                localRotation = localRotation?.Mirror();
                position = position?.Mirror();
                localPosition = localPosition?.Mirror();
            }

            if (scale.HasValue)
            {
                transform.localScale = scale.Value;
                ScaleUpdated?.Invoke();
            }

            if (localRotation.HasValue)
            {
                transform.localRotation = localRotation.Value;
                RotationUpdated?.Invoke();
            }
            else if (rotation.HasValue)
            {
                transform.rotation = rotation.Value;
                RotationUpdated?.Invoke();
            }

            if (localPosition.HasValue)
            {
                Vector3 localPositionValue = localPosition.Value;
                if (_v2)
                {
                    localPositionValue *= StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance;
                }

                transform.localPosition = localPositionValue;
                PositionUpdated?.Invoke();
            }
            else if (position.HasValue)
            {
                Vector3 positionValue = position.Value;
                if (_v2)
                {
                    positionValue *= StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance;
                }

                transform.position = positionValue;
                PositionUpdated?.Invoke();
            }
        }
    }

    public sealed class TransformControllerFactory : IDisposable
    {
        private readonly SiraLog _log;
        private readonly IInstantiator _instantiator;
        private readonly HashSet<TransformController> _transformControllers = new();

        [UsedImplicitly]
        private TransformControllerFactory(
            SiraLog log,
            IInstantiator instantiator)
        {
            _log = log;
            _instantiator = instantiator;
        }

        public void Dispose()
        {
            _transformControllers.DoIf(n => n != null, Object.Destroy);
        }

        public TransformController Create(GameObject gameObject, Track track, bool overwrite = false)
        {
            return Create(gameObject, new List<Track> { track }, overwrite);
        }

        public TransformController Create(GameObject gameObject, List<Track> track, bool overwrite = false)
        {
            TransformController existing = gameObject.GetComponent<TransformController>();
            if (existing != null)
            {
                if (overwrite)
                {
                    _log.Error($"Overwriting existing [{nameof(TransformController)}] on [{gameObject.name}]...");
                    Object.Destroy(existing);
                }
                else
                {
                    _log.Error($"Could not create [{nameof(TransformController)}], [{gameObject.name}] already has one");
                    return existing;
                }
            }

            TransformController controller = _instantiator.InstantiateComponent<TransformController>(gameObject, new object[] { track });
            _transformControllers.Add(controller);
            return controller;
        }
    }
}
