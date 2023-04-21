using UnityEngine;

namespace Heck.Animation
{
    internal class Vector4PointDefinitionInterpolation : PointDefinitionInterpolation<Vector4>
    {
        protected override Vector4 InterpolatePoints(PointDefinition<Vector4> previousPoint, PointDefinition<Vector4> basePoint, float interpolation, float time)
        {
            return Vector4.LerpUnclamped(previousPoint.Interpolate(time), basePoint.Interpolate(time), interpolation);
        }
    }
}
