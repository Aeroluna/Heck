using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using UnityEngine;
using static Heck.HeckController;

namespace Heck.Animation.Transform;

public readonly struct TransformData
{
    public TransformData(CustomData customData, bool v2 = false)
    {
        Scale = customData.GetVector3(v2 ? V2_SCALE : SCALE);
        Position = customData.GetVector3(v2 ? V2_POSITION : POSITION);
        Rotation = customData.GetQuaternion(v2 ? V2_ROTATION : ROTATION);
        LocalPosition = customData.GetVector3(v2 ? V2_LOCAL_POSITION : LOCAL_POSITION);
        LocalRotation = customData.GetQuaternion(v2 ? V2_LOCAL_ROTATION : LOCAL_ROTATION);
    }

    public Vector3? Scale { get; }

    public Vector3? Position { get; }

    public Quaternion? Rotation { get; }

    public Vector3? LocalPosition { get; }

    public Quaternion? LocalRotation { get; }

    public void Apply(UnityEngine.Transform transform, bool leftHanded, bool v2)
    {
        Vector3? position = Position;
        Vector3? localPosition = LocalPosition;
        if (v2)
        {
            // ReSharper disable once UseNullPropagation
            if (position.HasValue)
            {
                position = position.Value * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance;
            }

            // ReSharper disable once UseNullPropagation
            if (localPosition.HasValue)
            {
                localPosition = localPosition.Value * StaticBeatmapObjectSpawnMovementData.kNoteLinesDistance;
            }
        }

        Apply(transform, leftHanded, Scale, position, Rotation, localPosition, LocalRotation);
    }

    [PublicAPI]
    public void Apply(UnityEngine.Transform transform, bool leftHanded)
    {
        Apply(transform, leftHanded, Scale, Position, Rotation, LocalPosition, LocalRotation);
    }

    private static void Apply(
        UnityEngine.Transform transform,
        bool leftHanded,
        Vector3? scale,
        Vector3? position,
        Quaternion? rotation,
        Vector3? localPosition,
        Quaternion? localRotation)
    {
        if (leftHanded)
        {
            scale = scale?.Mirror();
            position = position?.Mirror();
            rotation = rotation?.Mirror();
            localPosition = localPosition?.Mirror();
            localRotation = localRotation?.Mirror();
        }

        if (scale.HasValue)
        {
            transform.localScale = scale.Value;
        }

        if (localPosition.HasValue)
        {
            transform.localPosition = localPosition.Value;
        }
        else if (position.HasValue)
        {
            transform.position = position.Value;
        }

        if (localRotation.HasValue)
        {
            transform.localRotation = localRotation.Value;
        }
        else if (rotation.HasValue)
        {
            transform.rotation = rotation.Value;
        }
    }
}
