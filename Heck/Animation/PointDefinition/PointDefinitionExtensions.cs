using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Heck.Animation
{
    public static class PointDefinitionExtensions
    {
        [Pure]
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

        [Pure]
        public static FloatPointDefinition ToPointDefinitionFloat(this List<object> list)
        {
            return new FloatPointDefinition(list);
        }

        [Pure]
        public static Vector3PointDefinition ToPointDefinitionVector3(this List<object> list)
        {
            return new Vector3PointDefinition(list);
        }

        [Pure]
        public static Vector4PointDefinition ToPointDefinitionVector4(this List<object> list)
        {
            return new Vector4PointDefinition(list);
        }

        [Pure]
        public static QuaternionPointDefinition ToPointDefinitionQuaternion(this List<object> list)
        {
            return new QuaternionPointDefinition(list);
        }

        // Equals was taken
        [Pure]
        public static bool EqualsTo(this float val1, float val2)
        {
            return Mathf.Approximately(val1, val2);
        }

        [Pure]
        public static bool EqualsTo(this Vector3 val1, Vector3 val2)
        {
            return val1 == val2;
        }

        [Pure]
        public static bool EqualsTo(this Vector4 val1, Vector4 val2)
        {
            return val1 == val2;
        }

        [Pure]
        public static bool EqualsTo(this Quaternion val1, Quaternion val2)
        {
            return Quaternion.Dot(val1, val2) >= 1;
        }
    }
}
