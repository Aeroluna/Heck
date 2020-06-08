using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NoodleExtensions.Animation
{
    internal class PointDataManager
    {
        internal Dictionary<string, PointData> _pointData { get; private set; } = new Dictionary<string, PointData>();

        internal void AddPoint(string pointDataName, PointData pointData)
        {
            if (!_pointData.TryGetValue(pointDataName, out _)) _pointData.Add(pointDataName, pointData);
            else Logger.Log($"Duplicate point defintion name, {pointDataName} could not be registered!", IPA.Logging.Logger.Level.Error);
        }
    }

    internal class PointData
    {
        private List<Vector4> _points = new List<Vector4>();

        internal void Add(Vector4 point) => _points.Add(point);

        internal Vector3 Interpolate(float time)
        {
            if (_points == null || _points.Count == 0) return Vector3.zero;
            if (time <= 0) return _points.First();
            int pointsCount = _points.Count;
            for (int i = 0; i < pointsCount; i++)
            {
                if (_points[i].w > time)
                {
                    if (i == 0) return _points.First();
                    return Vector3.Lerp(_points[i - 1], _points[i], (time - _points[i - 1].w) / (_points[i].w - _points[i - 1].w));
                }
            }
            return _points.Last();
        }

        internal Vector3 InterpolateAsQuaternion(float time)
        {
            if (_points == null || _points.Count == 0) return Vector3.zero;
            if (time <= 0) return _points.First();
            int pointsCount = _points.Count;
            for (int i = 0; i < pointsCount; i++)
            {
                if (_points[i].w > time)
                {
                    if (i == 0) return _points.First();
                    Quaternion quaternionOne = Quaternion.Euler(_points[i - 1]);
                    Quaternion quaternionTwo = Quaternion.Euler(_points[i]);
                    Quaternion quaternionFinal = Quaternion.Lerp(quaternionOne, quaternionTwo, (time - _points[i - 1].w) / (_points[i].w - _points[i - 1].w));
                    return quaternionFinal.eulerAngles;
                }
            }
            return _points.Last();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("{ ");
            _points.ForEach(n => stringBuilder.Append($"{n.ToString()} "));
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }
}