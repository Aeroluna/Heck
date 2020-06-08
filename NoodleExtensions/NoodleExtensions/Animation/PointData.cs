using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static NoodleExtensions.Plugin;

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
        private List<Vector4> _points;
        private List<Vector2> _linearPoints;

        internal void Add(Vector4 point)
        {
            if (_points == null) _points = new List<Vector4>();
            _points.Add(point);
        }

        internal void LinearAdd(Vector2 point)
        {
            if (_linearPoints == null) _linearPoints = new List<Vector2>();
            _linearPoints.Add(point);
        }

        internal Vector3 Interpolate(float time)
        {
            if (_points == null || _points.Count == 0) return _vectorZero;
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

        internal Quaternion InterpolateQuaternion(float time)
        {
            if (_points == null || _points.Count == 0) return _quaternionIdentity;
            if (time <= 0) return Quaternion.Euler(_points.First());
            int pointsCount = _points.Count;
            for (int i = 0; i < pointsCount; i++)
            {
                if (_points[i].w > time)
                {
                    if (i == 0) return Quaternion.Euler(_points.First());
                    Quaternion quaternionOne = Quaternion.Euler(_points[i - 1]);
                    Quaternion quaternionTwo = Quaternion.Euler(_points[i]);
                    return Quaternion.Lerp(quaternionOne, quaternionTwo, (time - _points[i - 1].w) / (_points[i].w - _points[i - 1].w));
                }
            }
            return Quaternion.Euler(_points.Last());
        }

        internal float InterpolateLinear(float time)
        {
            if (_linearPoints == null || _linearPoints.Count == 0) return 0;
            if (time <= 0) return _linearPoints.First().x;
            int pointsCount = _linearPoints.Count;
            for (int i = 0; i < pointsCount; i++)
            {
                if (_linearPoints[i].y > time)
                {
                    if (i == 0) return _linearPoints.First().x;
                    return Mathf.Lerp(_linearPoints[i - 1].x, _linearPoints[i].x, (time - _linearPoints[i - 1].y) / (_linearPoints[i].y - _linearPoints[i - 1].y));
                }
            }
            return _linearPoints.Last().x;
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