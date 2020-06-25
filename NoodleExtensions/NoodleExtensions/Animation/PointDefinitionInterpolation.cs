namespace NoodleExtensions.Animation
{
    using UnityEngine;

    internal class PointDefinitionInterpolation
    {
        private PointDefinition _basePointData;
        private PointDefinition _previousPointData;

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

            if (_previousPointData == null)
            {
                return _basePointData.Interpolate(time);
            }

            return Vector3.LerpUnclamped(_previousPointData.Interpolate(time), _basePointData.Interpolate(time), Time);
        }

        internal Quaternion? InterpolateQuaternion(float time)
        {
            if (_basePointData == null)
            {
                return null;
            }

            if (_previousPointData == null)
            {
                return _basePointData.InterpolateQuaternion(time);
            }

            return Quaternion.LerpUnclamped(_previousPointData.InterpolateQuaternion(time), _basePointData.InterpolateQuaternion(time), Time);
        }

        internal float? InterpolateLinear(float time)
        {
            if (_basePointData == null)
            {
                return null;
            }

            if (_previousPointData == null)
            {
                return _basePointData.InterpolateLinear(time);
            }

            return Mathf.LerpUnclamped(_previousPointData.InterpolateLinear(time), _basePointData.InterpolateLinear(time), Time);
        }

        internal void Init(PointDefinition newPointData)
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
