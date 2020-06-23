using System;
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
        private List<Point> _points;
        private List<Point> _linearPoints;

        private void Add(Point point)
        {
            if (_points == null) _points = new List<Point>();
            _points.Add(point);
        }

        private void LinearAdd(Point point)
        {
            if (_linearPoints == null) _linearPoints = new List<Point>();
            _linearPoints.Add(point);
        }

        internal Vector3 Interpolate(float time)
        {
            if (_points == null || _points.Count == 0) return _vectorZero;
            if (time <= 0) return _points.First()._point;
            int pointsCount = _points.Count;
            for (int i = 0; i < pointsCount; i++)
            {
                if (_points[i]._point.w > time)
                {
                    if (i == 0) return _points.First()._point;
                    float normalTime = (time - _points[i - 1]._point.w) / (_points[i]._point.w - _points[i - 1]._point.w);
                    normalTime = Easings.Interpolate(normalTime, _points[i]._easing);
                    if (_points[i]._smooth) return SmoothVectorLerp(_points, i - 1, i, normalTime);
                    else return Vector3.Lerp(_points[i - 1]._point, _points[i]._point, normalTime);
                }
            }
            return _points.Last()._point;
        }

        internal Quaternion InterpolateQuaternion(float time)
        {
            if (_points == null || _points.Count == 0) return _quaternionIdentity;
            if (time <= 0) return Quaternion.Euler(_points.First()._point);
            int pointsCount = _points.Count;
            for (int i = 0; i < pointsCount; i++)
            {
                if (_points[i]._point.w > time)
                {
                    if (i == 0) return Quaternion.Euler(_points.First()._point);
                    Quaternion quaternionOne = Quaternion.Euler(_points[i - 1]._point);
                    Quaternion quaternionTwo = Quaternion.Euler(_points[i]._point);
                    float normalTime = (time - _points[i - 1]._point.w) / (_points[i]._point.w - _points[i - 1]._point.w);
                    normalTime = Easings.Interpolate(normalTime, _points[i]._easing);
                    return Quaternion.Lerp(quaternionOne, quaternionTwo, normalTime);
                }
            }
            return Quaternion.Euler(_points.Last()._point);
        }

        // Kind of a sloppy way of implementing this, but hell if it works
        internal float InterpolateLinear(float time)
        {
            if (_linearPoints == null || _linearPoints.Count == 0) return 0;
            if (time <= 0) return _linearPoints.First()._linearPoint.x;
            int pointsCount = _linearPoints.Count;
            for (int i = 0; i < pointsCount; i++)
            {
                if (_linearPoints[i]._linearPoint.y > time)
                {
                    if (i == 0) return _linearPoints.First()._linearPoint.x;
                    float normalTime = (time - _linearPoints[i - 1]._linearPoint.y) / (_linearPoints[i]._linearPoint.y - _linearPoints[i - 1]._linearPoint.y);
                    normalTime = Easings.Interpolate(normalTime, _linearPoints[i]._easing);
                    return Mathf.Lerp(_linearPoints[i - 1]._linearPoint.x, _linearPoints[i]._linearPoint.x, normalTime);
                }
            }
            return _linearPoints.Last()._linearPoint.x;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("{ ");
            if (_points != null) _points.ForEach(n => stringBuilder.Append($"{n._point.ToString()} "));
            else if (_linearPoints != null) _linearPoints.ForEach(n => stringBuilder.Append($"{n._linearPoint.ToString()} "));
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }

        private static Vector3 SmoothVectorLerp(List<Point> points, int a, int b, float time)
        {
            // Catmull-Rom Spline
            Vector3 p0 = a - 1 < 0 ? points[a]._point : points[a - 1]._point;
            Vector3 p1 = points[a]._point;
            Vector3 p2 = points[b]._point;
            Vector3 p3 = b + 1 > points.Count - 1 ? points[b]._point : points[b + 1]._point;

            float t = time;

            float tt = t * t;
            float ttt = tt * t;

            float q0 = -ttt + 2.0f * tt - t;
            float q1 = 3.0f * ttt - 5.0f * tt + 2.0f;
            float q2 = -3.0f * ttt + 4.0f * tt + t;
            float q3 = ttt - tt;

            Vector3 c = 0.5f * (p0 * q0 + p1 * q1 + p2 * q2 + p3 * q3);

            return c;
        }

        internal static PointData DynamicToPointData(dynamic dyn)
        {
            IEnumerable<List<object>> points = ((IEnumerable<object>)dyn)
                        ?.Cast<List<object>>();
            if (points == null) return null;

            PointData pointData = new PointData();
            foreach (List<object> rawPoint in points)
            {
                int flagIndex = -1;
                int cachedCount = rawPoint.Count;
                for (int i = cachedCount - 1; i > 0; i--)
                {
                    if (rawPoint[i] is string) flagIndex = i;
                    else break;
                }
                Functions easing = Functions.easeLinear;
                bool spline = false;
                if (flagIndex != -1)
                {
                    List<string> flags = rawPoint.GetRange(flagIndex, cachedCount - flagIndex).Cast<string>().ToList();
                    rawPoint.RemoveRange(flagIndex, cachedCount - flagIndex);

                    string easingString = flags.Where(n => n.StartsWith("ease")).FirstOrDefault();
                    if (easingString != null) easing = (Functions)Enum.Parse(typeof(Functions), easingString);

                    // TODO: add more spicy splines
                    string splineString = flags.Where(n => n.StartsWith("spline")).FirstOrDefault();
                    if (splineString == "splineCatmullRom") spline = true;
                }

                if (rawPoint.Count() == 2)
                {
                    Vector2 vector = new Vector2(Convert.ToSingle(rawPoint[0]), Convert.ToSingle(rawPoint[1]));
                    pointData.LinearAdd(new Point(vector, easing));
                }
                else
                {
                    Vector4 vector = new Vector4(Convert.ToSingle(rawPoint[0]), Convert.ToSingle(rawPoint[1]), Convert.ToSingle(rawPoint[2]), Convert.ToSingle(rawPoint[3]));
                    pointData.Add(new Point(vector, easing, spline));
                }
            }
            return pointData;
        }

        private class Point
        {
            internal readonly Vector4 _point;
            internal readonly Vector2 _linearPoint;
            internal readonly Functions _easing;
            internal readonly bool _smooth;

            internal Point(Vector4 point, Functions easing = Functions.easeLinear, bool smooth = false)
            {
                _point = point;
                _easing = easing;
                _smooth = smooth;
            }

            internal Point(Vector2 point, Functions easing = Functions.easeLinear)
            {
                _linearPoint = point;
                _easing = easing;
            }
        }
    }

    internal class PointDataInterpolation
    {
        internal PointData _basePointData;
        internal PointData _previousPointData;
        internal Track _track;

        // used to interpolate from one path animation to another
        internal PointDataInterpolation(Track track)
        {
            _track = track;
        }

        internal Vector3? Interpolate(float time)
        {
            if (_basePointData == null) return null;
            if (_previousPointData == null) return _basePointData.Interpolate(time);
            return Vector3.Lerp(_previousPointData.Interpolate(time), _basePointData.Interpolate(time), _track._pathInterpolationTime);
        }

        internal Quaternion? InterpolateQuaternion(float time)
        {
            if (_basePointData == null) return null;
            if (_previousPointData == null) return _basePointData.InterpolateQuaternion(time);
            return Quaternion.Lerp(_previousPointData.InterpolateQuaternion(time), _basePointData.InterpolateQuaternion(time), _track._pathInterpolationTime);
        }

        internal float? InterpolateLinear(float time)
        {
            if (_basePointData == null) return null;
            if (_previousPointData == null) return _basePointData.InterpolateLinear(time);
            return Mathf.Lerp(_previousPointData.InterpolateLinear(time), _basePointData.InterpolateLinear(time), _track._pathInterpolationTime);
        }

        internal void Init(PointData newPointData)
        {
            _previousPointData = _basePointData ?? new PointData();
            _basePointData = newPointData;
        }

        internal void Finish()
        {
            _previousPointData = null;
        }

        public override string ToString()
        {
            return $"({_previousPointData?.ToString() ?? "null"}, {_basePointData?.ToString() ?? "null"})";
        }
    }
}
