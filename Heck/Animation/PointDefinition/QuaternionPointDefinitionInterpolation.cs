using UnityEngine;

namespace Heck.Animation;

internal class QuaternionPointDefinitionInterpolation : PointDefinitionInterpolation<Quaternion>
{
    protected override Quaternion InterpolatePoints(
        PointDefinition<Quaternion> previousPoint,
        PointDefinition<Quaternion> basePoint,
        float interpolation,
        float time)
    {
        return Quaternion.SlerpUnclamped(previousPoint.Interpolate(time), basePoint.Interpolate(time), interpolation);
    }
}
