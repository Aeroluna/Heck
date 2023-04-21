using UnityEngine;

namespace Heck.Animation
{
    internal class Vector3PointDefinitionInterpolation : PointDefinitionInterpolation<Vector3>
    {
        protected override Vector3 InterpolatePoints(PointDefinition<Vector3> previousPoint, PointDefinition<Vector3> basePoint, float interpolation, float time)
        {
            return Vector3.LerpUnclamped(previousPoint.Interpolate(time), basePoint.Interpolate(time), interpolation);
        }
    }
}
