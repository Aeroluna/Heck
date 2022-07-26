using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Heck.Animation
{
    public class PointDefinition
    {
        private readonly List<PointData> _points;

        public PointDefinition()
            : this(new List<PointData>())
        {
        }

        private PointDefinition(List<PointData> points)
        {
            _points = points;
        }

        public static PointDefinition ListToPointDefinition(List<object> list)
        {
            IEnumerable<List<object>> points = list.FirstOrDefault() is List<object> ? list.Cast<List<object>>() : new[] { list.Append(0).ToList() };

            List<PointData> pointData = new();
            foreach (List<object> rawPoint in points)
            {
                int flagIndex = -1;
                int cachedCount = rawPoint.Count;
                for (int i = cachedCount - 1; i > 0; i--)
                {
                    if (rawPoint[i] is string)
                    {
                        flagIndex = i;
                    }
                    else
                    {
                        break;
                    }
                }

                Functions easing = Functions.easeLinear;
                bool spline = false;
                HashSet<string> flags = new();
                List<object> copiedList = rawPoint.ToList();
                if (flagIndex != -1)
                {
                    flags = rawPoint.GetRange(flagIndex, cachedCount - flagIndex).Cast<string>().ToHashSet();
                    copiedList.RemoveRange(flagIndex, cachedCount - flagIndex);

                    string? easingString = flags.FirstOrDefault(n => n.StartsWith("ease"));
                    if (easingString != null)
                    {
                        easing = (Functions)Enum.Parse(typeof(Functions), easingString);
                    }

                    // TODO: add more spicy splines
                    string? splineString = flags.FirstOrDefault(n => n.StartsWith("spline"));
                    if (splineString == "splineCatmullRom")
                    {
                        spline = true;
                    }
                }

                bool hsv = flags.Any(n => n == "hsv");

                switch (copiedList.Count)
                {
                    case 2:
                    {
                        Vector2 vector = new(Convert.ToSingle(copiedList[0]), Convert.ToSingle(copiedList[1]));
                        pointData.Add(new PointData(vector, hsv, easing));
                        break;
                    }

                    case 4:
                    {
                        Vector4 vector = new(Convert.ToSingle(copiedList[0]), Convert.ToSingle(copiedList[1]), Convert.ToSingle(copiedList[2]), Convert.ToSingle(copiedList[3]));
                        pointData.Add(new PointData(vector, hsv, easing, spline));
                        break;
                    }

                    default:
                    {
                        Vector5 vector = new(Convert.ToSingle(copiedList[0]), Convert.ToSingle(copiedList[1]), Convert.ToSingle(copiedList[2]), Convert.ToSingle(copiedList[3]), Convert.ToSingle(copiedList[4]));
                        pointData.Add(new PointData(vector, hsv, easing));
                        break;
                    }
                }
            }

            return new PointDefinition(pointData);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new("{ ");
            _points.ForEach(n => stringBuilder.Append($"{n.Point} "));

            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }

        public Vector3 Interpolate(float time)
        {
            return Interpolate(time, out _);
        }

        public Vector3 Interpolate(float time, out bool last)
        {
            last = false;
            int count = _points.Count;
            if (count == 0)
            {
                return Vector3.zero;
            }

            Vector4 firstPoint = _points[0].Point;
            if (firstPoint.w >= time)
            {
                return firstPoint;
            }

            Vector4 lastPoint = _points[count - 1].Point;
            if (lastPoint.w <= time)
            {
                last = true;
                return lastPoint;
            }

            SearchIndex(time, PropertyType.Vector3, out int l, out int r);
            Vector4 pointL = _points[l].Point;
            Vector4 pointR = _points[r].Point;

            float normalTime;
            float divisor = pointR.w - pointL.w;
            if (divisor != 0)
            {
                normalTime = (time - pointL.w) / divisor;
            }
            else
            {
                normalTime = 0;
            }

            normalTime = Easings.Interpolate(normalTime, _points[r].Easing);
            return _points[r].Smooth ? SmoothVectorLerp(_points, l, r, normalTime)
                : Vector3.LerpUnclamped(pointL, pointR, normalTime);
        }

        public Quaternion InterpolateQuaternion(float time)
        {
            return InterpolateQuaternion(time, out _);
        }

        public Quaternion InterpolateQuaternion(float time, out bool last)
        {
            last = false;
            int count = _points.Count;
            if (count == 0)
            {
                return Quaternion.identity;
            }

            Vector5 firstPoint = _points[0].Vector4Point;
            if (firstPoint.v >= time)
            {
                return firstPoint;
            }

            Vector5 lastPoint = _points[count - 1].Vector4Point;
            if (lastPoint.v <= time)
            {
                last = true;
                return lastPoint;
            }

            SearchIndex(time, PropertyType.Quaternion, out int l, out int r);
            Vector5 pointL = _points[l].Vector4Point;
            Vector5 pointR = _points[r].Vector4Point;
            Quaternion quaternionOne = pointL;
            Quaternion quaternionTwo = pointR;

            float normalTime;
            float divisor = pointR.v - pointL.v;
            if (divisor != 0)
            {
                normalTime = (time - pointL.v) / divisor;
            }
            else
            {
                normalTime = 0;
            }

            normalTime = Easings.Interpolate(normalTime, _points[r].Easing);
            return Quaternion.SlerpUnclamped(quaternionOne, quaternionTwo, normalTime);
        }

        public float InterpolateLinear(float time)
        {
            return InterpolateLinear(time, out _);
        }

        // Kind of a sloppy way of implementing this, but hell if it works
        public float InterpolateLinear(float time, out bool last)
        {
            last = false;
            int count = _points.Count;
            if (count == 0)
            {
                return 0;
            }

            Vector2 firstPoint = _points[0].LinearPoint;
            if (firstPoint.y >= time)
            {
                return firstPoint.x;
            }

            Vector2 lastPoint = _points[count - 1].LinearPoint;
            if (lastPoint.y <= time)
            {
                last = true;
                return lastPoint.x;
            }

            SearchIndex(time, PropertyType.Linear, out int l, out int r);
            Vector4 pointL = _points[l].LinearPoint;
            Vector4 pointR = _points[r].LinearPoint;

            float normalTime;
            float divisor = pointR.y - pointL.y;
            if (divisor != 0)
            {
                normalTime = (time - pointL.y) / divisor;
            }
            else
            {
                normalTime = 0;
            }

            normalTime = Easings.Interpolate(normalTime, _points[r].Easing);
            return Mathf.LerpUnclamped(pointL.x, pointR.x, normalTime);
        }

        public Vector4 InterpolateVector4(float time)
        {
            return InterpolateVector4(time, out _);
        }

        public Vector4 InterpolateVector4(float time, out bool last)
        {
            last = false;
            int count = _points.Count;
            if (count == 0)
            {
                return Vector4.zero;
            }

            Vector5 firstPoint = _points[0].Vector4Point;
            if (firstPoint.v >= time)
            {
                return firstPoint;
            }

            Vector5 lastPoint = _points[count - 1].Vector4Point;
            if (lastPoint.v <= time)
            {
                last = true;
                return lastPoint;
            }

            SearchIndex(time, PropertyType.Vector4, out int l, out int r);
            PointData pointDataL = _points[l];
            PointData pointDataR = _points[r];
            Vector5 pointL = pointDataL.Vector4Point;
            Vector5 pointR = pointDataR.Vector4Point;

            float normalTime;
            float divisor = pointR.v - pointL.v;
            if (divisor != 0)
            {
                normalTime = (time - pointL.v) / divisor;
            }
            else
            {
                normalTime = 0;
            }

            normalTime = Easings.Interpolate(normalTime, pointDataR.Easing);

            // If next point is HSV, convert both values to HSV
            bool hsv = pointDataR.HSV;
            if (hsv)
            {
                pointL = pointL.ToHSV();
                pointR = pointR.ToHSV();
            }

            Vector4 result = Vector4.LerpUnclamped(pointL, pointR, normalTime);

            // RGB lerp
            if (!hsv)
            {
                return result;
            }

            // HSV lerp, convert to RGB
            float alpha = result.w;
            result = Color.HSVToRGB(result.x, result.y, result.y, true);
            result.w = alpha;

            return result;
        }

        private static Vector3 SmoothVectorLerp(List<PointData> points, int a, int b, float time)
        {
            // Catmull-Rom Spline
            Vector3 p0 = a - 1 < 0 ? points[a].Point : points[a - 1].Point;
            Vector3 p1 = points[a].Point;
            Vector3 p2 = points[b].Point;
            Vector3 p3 = b + 1 > points.Count - 1 ? points[b].Point : points[b + 1].Point;

            float tt = time * time;
            float ttt = tt * time;

            float q0 = -ttt + (2.0f * tt) - time;
            float q1 = (3.0f * ttt) - (5.0f * tt) + 2.0f;
            float q2 = (-3.0f * ttt) + (4.0f * tt) + time;
            float q3 = ttt - tt;

            Vector3 c = 0.5f * ((p0 * q0) + (p1 * q1) + (p2 * q2) + (p3 * q3));

            return c;
        }

        // Use binary search instead of linear search.
        private void SearchIndex(float time, PropertyType propertyType, out int l, out int r)
        {
            l = 0;
            r = _points.Count;

            while (l < r - 1)
            {
                int m = (l + r) / 2;
                float pointTime = 0;
                switch (propertyType)
                {
                    case PropertyType.Linear:
                        pointTime = _points[m].LinearPoint.y;
                        break;

                    case PropertyType.Quaternion:
                    case PropertyType.Vector3:
                        pointTime = _points[m].Point.w;
                        break;

                    case PropertyType.Vector4:
                        pointTime = _points[m].Vector4Point.v;
                        break;
                }

                if (pointTime < time)
                {
                    l = m;
                }
                else
                {
                    r = m;
                }
            }
        }

        private readonly struct Vector5
        {
            private readonly float x;

            private readonly float y;

            private readonly float z;

            private readonly float w;

            internal Vector5(float x, float y, float z, float w, float v)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
                this.v = v;
            }

#pragma warning disable SA1300 // Element should begin with upper-case letter
            internal float v { get; }
#pragma warning restore SA1300 // Element should begin with upper-case letter

            public static implicit operator Vector4(Vector5 vector)
            {
                return new Vector4(vector.x, vector.y, vector.z, vector.w);
            }

            public static implicit operator Quaternion(Vector5 vector)
            {
                return new Quaternion(vector.x, vector.y, vector.z, vector.w);
            }

            public Vector5 ToHSV()
            {
                Color c = new(x, y, z, v);
                Color.RGBToHSV(c, out float h, out float s, out float b);
                return new Vector5(h, s, b, w, v);
            }
        }

        private class PointData
        {
            internal PointData(Vector4 point, bool hsv, Functions easing = Functions.easeLinear, bool smooth = false)
            {
                Point = point;
                HSV = hsv;
                Easing = easing;
                Smooth = smooth;
                Quaternion quaternion = Quaternion.Euler(point);
                Vector4Point = new Vector5(quaternion.x, quaternion.y, quaternion.z, quaternion.w, point.w);
            }

            internal PointData(Vector2 point, bool hsv, Functions easing = Functions.easeLinear)
            {
                LinearPoint = point;
                HSV = hsv;
                Easing = easing;
            }

            internal PointData(Vector5 point, bool hsv, Functions easing = Functions.easeLinear)
            {
                Vector4Point = point;
                HSV = hsv;
                Easing = easing;
            }

            internal Vector4 Point { get; }

            internal Vector5 Vector4Point { get; }

            internal Vector2 LinearPoint { get; }

            internal Functions Easing { get; }

            internal bool Smooth { get; }

            internal bool HSV { get; }
        }
    }
}
