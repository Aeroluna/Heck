namespace Heck
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using UnityEngine;

    public static class NullableExtensions
    {
        public static IEnumerable<float?>? GetNullableFloats(this Dictionary<string, object?> dynData, string key)
        {
            return dynData.Get<List<object>>(key)?.Select(n => n.ToNullableFloat());
        }

        public static float? ToNullableFloat(this object @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToSingle(@this);
        }

        public static Vector3? SumVectorNullables(params Vector3?[] vectors)
        {
            IEnumerable<Vector3> validVectors = vectors.Where(n => n.HasValue).Select(n => n!.Value);
            if (validVectors.Any())
            {
                Vector3 total = Vector3.zero;
                foreach (Vector3 vector in validVectors)
                {
                    total += vector;
                }

                return total;
            }

            return null;
        }

        public static Vector3? MultVectorNullables(params Vector3?[] vectors)
        {
            IEnumerable<Vector3> validVectors = vectors.Where(n => n.HasValue).Select(n => n!.Value);
            if (validVectors.Any())
            {
                Vector3 total = Vector3.one;
                foreach (Vector3 vector in validVectors)
                {
                    total = Vector3.Scale(total, vector);
                }

                return total;
            }

            return null;
        }

        public static Quaternion? MultQuaternionNullables(params Quaternion?[] quaternions)
        {
            IEnumerable<Quaternion> validQuaternions = quaternions.Where(n => n.HasValue).Select(n => n!.Value);
            if (validQuaternions.Any())
            {
                Quaternion total = Quaternion.identity;
                foreach (Quaternion quaternion in validQuaternions)
                {
                    total *= quaternion;
                }

                return total;
            }

            return null;
        }

        public static float? MultFloatNullables(params float?[] floats)
        {
            IEnumerable<float> validFloats = floats.Where(n => n.HasValue).Select(n => n!.Value);
            if (validFloats.Any())
            {
                float total = 1;
                foreach (float @float in validFloats)
                {
                    total *= @float;
                }

                return total;
            }

            return null;
        }

        public static Vector4? MultVector4Nullables(params Vector4?[] vectors)
        {
            IEnumerable<Vector4> validVectors = vectors.Where(n => n.HasValue).Select(n => n!.Value);
            if (validVectors.Any())
            {
                Vector4 total = Vector4.one;
                foreach (Vector4 vector in validVectors)
                {
                    total = Vector4.Scale(total, vector);
                }

                return total;
            }

            return null;
        }

        public static void MirrorVectorNullable(ref Vector3? vector)
        {
            if (vector.HasValue)
            {
                Vector3 modifiedVector = vector.Value;
                modifiedVector.x *= -1;
                vector = modifiedVector;
            }
        }

        public static void MirrorQuaternionNullable(ref Quaternion? quaternion)
        {
            if (quaternion.HasValue)
            {
                Quaternion modifiedVector = quaternion.Value;
                quaternion = new Quaternion(modifiedVector.x, modifiedVector.y * -1, modifiedVector.z * -1, modifiedVector.w);
            }
        }
    }
}
