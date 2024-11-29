using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using JetBrains.Annotations;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;
using static Heck.HeckController;
using static Heck.NullableExtensions;
using Object = UnityEngine.Object;

namespace Heck.Animation.Transform;

// General controller script for GameObjects
public class TransformController : MonoBehaviour
{
    private bool _leftHanded;
    private bool _v2;

    public event Action? PositionUpdated;

    public event Action? RotationUpdated;

    public event Action? ScaleUpdated;

    public List<Track> Tracks { get; set; } = [];

    [Inject]
    [UsedImplicitly]
    private void Construct(
        [Inject(Id = LEFT_HANDED_ID)] bool leftHanded,
        IReadonlyBeatmapData beatmapData)
    {
        _leftHanded = leftHanded;
        if (beatmapData is IVersionable versionable)
        {
            _v2 = versionable.version.IsVersion2();
        }
    }

    private void OnEnable()
    {
        UpdatePos();
    }

    private void OnTransformParentChanged()
    {
        UpdatePos();
    }

    // This method runs on each frame in the game scene, so avoid allocations (do NOT use Linq).
    private void Update()
    {
        // ReSharper disable once ForCanBeConvertedToForeach
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (int i = 0; i < Tracks.Count; i++)
        {
            if (!Tracks[i].UpdatedThisFrame)
            {
                continue;
            }

            UpdatePos();
            return;
        }
    }

    // This method runs on each frame in the game scene, so avoid allocations (do NOT use Linq).
    private void UpdatePos()
    {
        int count = Tracks.Count;
        if (count == 0)
        {
            return;
        }

        Vector3? scale;
        Quaternion? rotation;
        Quaternion? localRotation;
        Vector3? position;
        Vector3? localPosition;

        if (count > 1)
        {
            Vector3? multScale = null;
            Quaternion? multRotation = null;
            Quaternion? multLocalRotation = null;
            Vector3? sumPosition = null;
            Vector3? sumLocalPosition = null;

            foreach (Track track in Tracks)
            {
                multScale = MultVectorNullables(multScale, track.GetProperty<Vector3>(SCALE));
                multRotation = MultQuaternionNullables(multRotation, track.GetProperty<Quaternion>(ROTATION));
                multLocalRotation = MultQuaternionNullables(
                    multLocalRotation,
                    track.GetProperty<Quaternion>(LOCAL_ROTATION));
                sumPosition = SumVectorNullables(sumPosition, track.GetProperty<Vector3>(POSITION));
                sumLocalPosition = SumVectorNullables(sumLocalPosition, track.GetProperty<Vector3>(LOCAL_POSITION));
            }

            scale = multScale;
            rotation = multRotation;
            localRotation = multLocalRotation;
            position = sumPosition;
            localPosition = sumLocalPosition;
        }
        else
        {
            Track track = Tracks.First();
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
    private readonly IInstantiator _instantiator;
    private readonly SiraLog _log;
    private readonly HashSet<TransformController> _transformControllers = [];

    [UsedImplicitly]
    private TransformControllerFactory(
        SiraLog log,
        IInstantiator instantiator)
    {
        _log = log;
        _instantiator = instantiator;
    }

    public TransformController Create(GameObject gameObject, Track track, bool overwrite = false)
    {
        return Create(gameObject, [track], overwrite);
    }

    public TransformController Create(GameObject gameObject, List<Track> track, bool overwrite = false)
    {
        TransformController existing = gameObject.GetComponent<TransformController>();
        if (existing != null)
        {
            if (overwrite)
            {
                _log.Error($"Overwriting existing [{nameof(TransformController)}] on [{gameObject.name}]...");
                existing.Tracks = track;
            }
            else
            {
                _log.Error($"Could not create [{nameof(TransformController)}], [{gameObject.name}] already has one");
            }

            return existing;
        }

        TransformController controller =
            _instantiator.InstantiateComponent<TransformController>(gameObject);
        controller.Tracks = track;
        _transformControllers.Add(controller);
        return controller;
    }

    public void Dispose()
    {
        _transformControllers.DoIf(n => n != null, Object.Destroy);
    }
}
