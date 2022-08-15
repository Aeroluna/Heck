using System;
using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static Heck.HeckController;
using Logger = IPA.Logging.Logger;
using Object = UnityEngine.Object;

namespace Heck.Animation.Transform
{
    // General controller script for GameObjects
    public class TransformController : MonoBehaviour
    {
        private bool _v2;
        private bool _leftHanded;
        private Track _track = null!;

        public event Action? RotationUpdated;

        public event Action? PositionUpdated;

        public event Action? ScaleUpdated;

        [Inject]
        [UsedImplicitly]
        private void Construct(
            [Inject(Id = LEFT_HANDED_ID)] bool leftHanded,
            IReadonlyBeatmapData beatmapData,
            Track track)
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
            if (_track.UpdatedThisFrame)
            {
                UpdatePos();
            }
        }

        private void UpdatePos()
        {
            Vector3? scale = GetVectorNullable(SCALE);
            Quaternion? rotation = GetQuaternionNullable(ROTATION);
            Quaternion? localRotation = GetQuaternionNullable(LOCAL_ROTATION);
            Vector3? position = GetVectorNullable(POSITION);
            Vector3? localPosition = GetVectorNullable(LOCAL_POSITION);

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

        private Vector3? GetVectorNullable(string property) => _track.GetProperty<Vector3>(property)?.Mirror(_leftHanded);

        private Quaternion? GetQuaternionNullable(string property) => _track.GetProperty<Quaternion>(property)?.Mirror(_leftHanded);
    }

    public sealed class TransformControllerFactory : IDisposable
    {
        private readonly IInstantiator _instantiator;
        private readonly HashSet<TransformController> _transformControllers = new();

        [UsedImplicitly]
        private TransformControllerFactory(
            IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }

        public void Dispose()
        {
            _transformControllers.DoIf(n => n != null, Object.Destroy);
        }

        public TransformController Create(GameObject gameObject, Track track, bool overwrite = false)
        {
            TransformController existing = gameObject.GetComponent<TransformController>();
            if (existing != null)
            {
                if (overwrite)
                {
                    Log.Logger.Log($"Overwriting existing [{nameof(TransformController)}] on [{gameObject.name}]...", Logger.Level.Error);
                    Object.Destroy(existing);
                }
                else
                {
                    Log.Logger.Log($"Could not create [{nameof(TransformController)}], [{gameObject.name}] already has one.", Logger.Level.Error);
                    return existing;
                }
            }

            TransformController controller = _instantiator.InstantiateComponent<TransformController>(gameObject, new object[] { track });
            _transformControllers.Add(controller);
            return controller;
        }
    }
}
