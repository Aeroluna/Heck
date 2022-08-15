using System;

namespace Heck.Animation
{
    internal interface IPointDefinitionInterpolation
    {
        public float Time { set; }

        public void Finish();

        public void Init(IPointDefinition pointDefinition);
    }

    internal class PointDefinitionInterpolation<T> : IPointDefinitionInterpolation
        where T : struct
    {
        private PointDefinition<T>? _basePointData;
        private PointDefinition<T>? _previousPointData;

        public float Time { get; set; }

        public override string ToString()
        {
            return $"({_previousPointData?.ToString() ?? "null"}, {_basePointData?.ToString() ?? "null"})";
        }

        public void Finish()
        {
            _previousPointData = null;
        }

        public void Init(IPointDefinition? newPointData)
        {
            Time = 0;
            _previousPointData = _basePointData ?? new PointDefinition<T>();
            if (newPointData == null)
            {
                _basePointData = null;
                return;
            }

            if (newPointData is not PointDefinition<T> casted)
            {
                throw new InvalidOperationException();
            }

            _basePointData = casted;
        }

        internal T? Interpolate(float time, Func<PointDefinition<T>?, PointDefinition<T>, float, float, T> func)
        {
            if (_basePointData == null)
            {
                return null;
            }

            return func(_previousPointData, _basePointData, Time, time);
        }
    }
}
