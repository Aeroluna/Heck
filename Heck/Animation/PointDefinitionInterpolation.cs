using UnityEngine;

namespace Heck.Animation
{
    internal class PointDefinitionInterpolation
    {
        private PointDefinition? _basePointData;
        private PointDefinition? _previousPointData;

        internal float Time { get; set; }

        public override string ToString()
        {
            return $"({_previousPointData?.ToString() ?? "null"}, {_basePointData?.ToString() ?? "null"})";
        }

        internal Vector3? Interpolate(float time)
        {
            if (_basePointData == null)
            {
                return null;
            }

            return _previousPointData == null ? _basePointData.Interpolate(time)
                : Vector3.LerpUnclamped(_previousPointData.Interpolate(time), _basePointData.Interpolate(time), Time);
        }

        internal Quaternion? InterpolateQuaternion(float time)
        {
            if (_basePointData == null)
            {
                return null;
            }

            return _previousPointData == null ? _basePointData.InterpolateQuaternion(time)
                : Quaternion.SlerpUnclamped(_previousPointData.InterpolateQuaternion(time), _basePointData.InterpolateQuaternion(time), Time);
        }

        internal float? InterpolateLinear(float time)
        {
            if (_basePointData == null)
            {
                return null;
            }

            return _previousPointData == null ? _basePointData.InterpolateLinear(time)
                : Mathf.LerpUnclamped(_previousPointData.InterpolateLinear(time), _basePointData.InterpolateLinear(time), Time);
        }

        internal Vector4? InterpolateVector4(float time)
        {
            if (_basePointData == null)
            {
                return null;
            }

            return _previousPointData == null ? _basePointData.InterpolateVector4(time)
                : Vector4.LerpUnclamped(_previousPointData.InterpolateVector4(time), _basePointData.InterpolateVector4(time), Time);
        }

        internal void Init(PointDefinition? newPointData)
        {
            Time = 0;
            _previousPointData = _basePointData ?? new PointDefinition();
            _basePointData = newPointData;
        }

        internal void Finish()
        {
            _previousPointData = null;
        }
    }
}
