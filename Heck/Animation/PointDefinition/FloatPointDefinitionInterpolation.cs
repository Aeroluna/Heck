using UnityEngine;

namespace Heck.Animation
{
    internal class FloatPointDefinitionInterpolation : PointDefinitionInterpolation<float>
    {
        protected override float InterpolatePoints(PointDefinition<float> previousPoint, PointDefinition<float> basePoint, float interpolation, float time)
        {
            return Mathf.LerpUnclamped(previousPoint.Interpolate(time), basePoint.Interpolate(time), interpolation);
        }
    }
}
