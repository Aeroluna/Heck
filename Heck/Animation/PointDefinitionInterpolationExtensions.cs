using UnityEngine;

namespace Heck.Animation
{
    internal static class PointDefinitionInterpolationExtensions
    {
        internal static float? Interpolate(this PointDefinitionInterpolation<float> pointDefinition, float time)
        {
            return pointDefinition.Interpolate(time, HandleInterpolateFloat);
        }

        internal static Vector3? Interpolate(this PointDefinitionInterpolation<Vector3> pointDefinition, float time)
        {
            return pointDefinition.Interpolate(time, HandleInterpolateVector3);
        }

        internal static Vector4? Interpolate(this PointDefinitionInterpolation<Vector4> pointDefinition, float time)
        {
            return pointDefinition.Interpolate(time, HandleInterpolateVector4);
        }

        internal static Quaternion? Interpolate(this PointDefinitionInterpolation<Quaternion> pointDefinition, float time)
        {
            return pointDefinition.Interpolate(time, HandleInterpolateQuaternion);
        }

        private static float HandleInterpolateFloat(PointDefinition<float>? previousPoint, PointDefinition<float> basePoint, float interpolation, float time)
        {
            return previousPoint == null ? basePoint.Interpolate(time)
                : Mathf.LerpUnclamped(previousPoint.Interpolate(time), basePoint.Interpolate(time), interpolation);
        }

        private static Vector3 HandleInterpolateVector3(PointDefinition<Vector3>? previousPoint, PointDefinition<Vector3> basePoint, float interpolation, float time)
        {
            return previousPoint == null ? basePoint.Interpolate(time)
                : Vector3.LerpUnclamped(previousPoint.Interpolate(time), basePoint.Interpolate(time), interpolation);
        }

        private static Vector4 HandleInterpolateVector4(PointDefinition<Vector4>? previousPoint, PointDefinition<Vector4> basePoint, float interpolation, float time)
        {
            return previousPoint == null ? basePoint.Interpolate(time)
                : Vector4.LerpUnclamped(previousPoint.Interpolate(time), basePoint.Interpolate(time), interpolation);
        }

        private static Quaternion HandleInterpolateQuaternion(PointDefinition<Quaternion>? previousPoint, PointDefinition<Quaternion> basePoint, float interpolation, float time)
        {
            return previousPoint == null ? basePoint.Interpolate(time)
                : Quaternion.SlerpUnclamped(previousPoint.Interpolate(time), basePoint.Interpolate(time), interpolation);
        }
    }
}
