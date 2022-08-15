using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heck.Animation
{
    public static class PointDefinitionExtensions
    {
        private delegate void PointDefinitionHandler<T>(List<object> list, out T value, out float time);

        public static float Interpolate(this PointDefinition<float> pointDefinition, float time)
        {
            return pointDefinition.Interpolate(time, out _);
        }

        public static Vector3 Interpolate(this PointDefinition<Vector3> pointDefinition, float time)
        {
            return pointDefinition.Interpolate(time, out _);
        }

        public static Vector4 Interpolate(this PointDefinition<Vector4> pointDefinition, float time)
        {
            return pointDefinition.Interpolate(time, out _);
        }

        public static Quaternion Interpolate(this PointDefinition<Quaternion> pointDefinition, float time)
        {
            return pointDefinition.Interpolate(time, out _);
        }

        public static float Interpolate(this PointDefinition<float> pointDefinition, float time, out bool last)
        {
            return pointDefinition.Interpolate(time, HandleInterpolateFloat, out last);
        }

        public static Vector3 Interpolate(this PointDefinition<Vector3> pointDefinition, float time, out bool last)
        {
            return pointDefinition.Interpolate(time, HandleInterpolateVector3, out last);
        }

        public static Vector4 Interpolate(this PointDefinition<Vector4> pointDefinition, float time, out bool last)
        {
            return pointDefinition.Interpolate(time, HandleInterpolateVector4, out last);
        }

        public static Quaternion Interpolate(this PointDefinition<Quaternion> pointDefinition, float time, out bool last)
        {
            return pointDefinition.Interpolate(time, HandleInterpolateQuaternion, out last);
        }

        public static PointDefinition<T> ToPointDefinition<T>(this List<object> list)
            where T : struct
        {
            PointDefinition<T>? result = typeof(T) switch
            {
                var n when n == typeof(float) => list.ToPointDefinitionFloat() as PointDefinition<T>,
                var n when n == typeof(Vector3) => list.ToPointDefinitionVector3() as PointDefinition<T>,
                var n when n == typeof(Vector4) => list.ToPointDefinitionVector4() as PointDefinition<T>,
                var n when n == typeof(Quaternion) => list.ToPointDefinitionQuaternion() as PointDefinition<T>,
                _ => throw new ArgumentOutOfRangeException(nameof(T))
            };

            return result ?? throw new InvalidCastException("Invalid cast.");
        }

        public static PointDefinition<float> ToPointDefinitionFloat(this List<object> list)
        {
            return list.ToPointDefinition<float>(HandleFloat);
        }

        public static PointDefinition<Vector3> ToPointDefinitionVector3(this List<object> list)
        {
            return list.ToPointDefinition<Vector3>(HandleVector3);
        }

        public static PointDefinition<Vector4> ToPointDefinitionVector4(this List<object> list)
        {
            return list.ToPointDefinition<Vector4>(HandleVector4);
        }

        public static PointDefinition<Quaternion> ToPointDefinitionQuaternion(this List<object> list)
        {
            return list.ToPointDefinition<Quaternion>(HandleQuaternion);
        }

        private static float HandleInterpolateFloat(List<PointDefinition<float>.PointData> points, int l, int r, float time)
        {
            return Mathf.LerpUnclamped(points[l].Point, points[r].Point, time);
        }

        private static Vector3 HandleInterpolateVector3(List<PointDefinition<Vector3>.PointData> points, int l, int r, float time)
        {
            PointDefinition<Vector3>.PointData pointR = points[r];
            return pointR.Smooth ? SmoothVectorLerp(points, l, r, time)
                : Vector3.LerpUnclamped(points[l].Point, pointR.Point, time);
        }

        private static Vector4 HandleInterpolateVector4(List<PointDefinition<Vector4>.PointData> points, int l, int r, float time)
        {
            return Vector4.LerpUnclamped(points[l].Point, points[r].Point, time);
        }

        private static Quaternion HandleInterpolateQuaternion(List<PointDefinition<Quaternion>.PointData> points, int l, int r, float time)
        {
            return Quaternion.SlerpUnclamped(points[l].Point, points[r].Point, time);
        }

        private static void HandleFloat(List<object> copiedList, out float value, out float time)
        {
            value = Convert.ToSingle(copiedList[0]);
            time = Convert.ToSingle(copiedList[1]);
        }

        private static void HandleVector3(List<object> copiedList, out Vector3 value, out float time)
        {
            value = new Vector3(Convert.ToSingle(copiedList[0]), Convert.ToSingle(copiedList[1]), Convert.ToSingle(copiedList[2]));
            time = Convert.ToSingle(copiedList[3]);
        }

        private static void HandleVector4(List<object> copiedList, out Vector4 value, out float time)
        {
            value = new Vector4(Convert.ToSingle(copiedList[0]), Convert.ToSingle(copiedList[1]), Convert.ToSingle(copiedList[2]), Convert.ToSingle(copiedList[3]));
            time = Convert.ToSingle(copiedList[4]);
        }

        private static void HandleQuaternion(List<object> copiedList, out Quaternion value, out float time)
        {
            value = Quaternion.Euler(new Vector3(Convert.ToSingle(copiedList[0]), Convert.ToSingle(copiedList[1]), Convert.ToSingle(copiedList[2])));
            time = Convert.ToSingle(copiedList[3]);
        }

        private static PointDefinition<T> ToPointDefinition<T>(this List<object> list, PointDefinitionHandler<T> func)
            where T : struct
        {
            IEnumerable<List<object>> points = list.FirstOrDefault() is List<object> ? list.Cast<List<object>>() : new[] { list.Append(0).ToList() };

            List<PointDefinition<T>.PointData> pointData = new();
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
                List<object> copiedList = rawPoint.ToList();
                if (flagIndex != -1)
                {
                    List<string> flags = rawPoint.GetRange(flagIndex, cachedCount - flagIndex).Cast<string>().ToList();
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

                func(copiedList, out T value, out float time);
                pointData.Add(new PointDefinition<T>.PointData(value, time, easing, spline));
            }

            return new PointDefinition<T>(pointData);
        }

        private static Vector3 SmoothVectorLerp(List<PointDefinition<Vector3>.PointData> points, int a, int b, float time)
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
    }
}
