using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using UnityEngine;
using static Heck.HeckController;

namespace Heck.Animation.Transform
{
    public readonly struct TransformData
    {
        public readonly Vector3? Scale;
        public readonly Vector3? Position;
        public readonly Vector3? Rotation;
        public readonly Vector3? LocalPosition;
        public readonly Vector3? LocalRotation;

        public TransformData(CustomData customData, bool v2 = false)
        {
            Scale = customData.GetVector3(v2 ? V2_SCALE : SCALE);
            Position = customData.GetVector3(v2 ? V2_POSITION : POSITION);
            Rotation = customData.GetVector3(v2 ? V2_ROTATION : ROTATION);
            LocalPosition = customData.GetVector3(v2 ? V2_LOCAL_POSITION : LOCAL_POSITION);
            LocalRotation = customData.GetVector3(v2 ? V2_LOCAL_ROTATION : LOCAL_ROTATION);
        }

        public void Apply(UnityEngine.Transform transform, bool leftHanded, bool v2, float noteLinesDistance)
        {
            Vector3? position = Position;
            Vector3? localPosition = LocalPosition;
            if (v2)
            {
                // ReSharper disable once UseNullPropagation
                if (position.HasValue)
                {
                    position = position.Value * noteLinesDistance;
                }

                // ReSharper disable once UseNullPropagation
                if (localPosition.HasValue)
                {
                    localPosition = localPosition.Value * noteLinesDistance;
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
            Vector3? rotation,
            Vector3? localPosition,
            Vector3? localRotation)
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

            if (position.HasValue)
            {
                transform.position = position.Value;
            }
            else if (localPosition.HasValue)
            {
                transform.localPosition = localPosition.Value;
            }

            if (rotation.HasValue)
            {
                transform.eulerAngles = rotation.Value;
            }
            else if (localRotation.HasValue)
            {
                transform.localEulerAngles = localRotation.Value;
            }
        }
    }
}
