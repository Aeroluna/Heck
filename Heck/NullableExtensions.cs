using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using JetBrains.Annotations;
using UnityEngine;

namespace Heck
{
    public static class NullableExtensions
    {
        public static IEnumerable<float?>? GetNullableFloats(this Dictionary<string, object?> dynData, string key)
        {
            return dynData.Get<List<object>>(key)?.Select(n => n.ToNullableFloat());
        }

        [PublicAPI]
        public static float? ToNullableFloat(this object? @this)
        {
            if (@this == null || @this == DBNull.Value)
            {
                return null;
            }

            return Convert.ToSingle(@this);
        }

        public static Vector3? SumVectorNullables(Vector3? vectorOne, Vector3? vectorTwo)
        {
            if (!vectorOne.HasValue && !vectorTwo.HasValue)
            {
                return null;
            }

            Vector3 total = Vector3.zero;
            if (vectorOne.HasValue)
            {
                total += vectorOne.Value;
            }

            if (vectorTwo.HasValue)
            {
                total += vectorTwo.Value;
            }

            return total;
        }

        public static Vector3? MultVectorNullables(Vector3? vectorOne, Vector3? vectorTwo)
        {
            if (vectorOne.HasValue)
            {
                return vectorTwo.HasValue ? Vector3.Scale(vectorOne.Value, vectorTwo.Value) : vectorOne;
            }

            return vectorTwo;
        }

        public static Quaternion? MultQuaternionNullables(Quaternion? quaternionOne, Quaternion? quaternionTwo)
        {
            if (quaternionOne.HasValue)
            {
                return quaternionTwo.HasValue ? quaternionOne.Value * quaternionTwo.Value : quaternionOne;
            }

            return quaternionTwo;
        }

        public static float? MultFloatNullables(float? floatOne, float? floatTwo)
        {
            if (floatOne.HasValue)
            {
                return floatTwo.HasValue ? floatOne.Value * floatTwo.Value : floatOne;
            }

            return floatTwo;
        }

        public static Vector4? MultVector4Nullables(Vector4? vectorOne, Vector4? vectorTwo)
        {
            if (vectorOne.HasValue)
            {
                return vectorTwo.HasValue ? Vector4.Scale(vectorOne.Value, vectorTwo.Value) : vectorOne;
            }

            return vectorTwo;
        }

        public static Vector3? SumVectorNullables(IEnumerable<Vector3?> vectors)
        {
            bool valid = false;
            Vector3 total = Vector3.zero;

            foreach (Vector3? nullable in vectors)
            {
                if (!nullable.HasValue)
                {
                    continue;
                }

                total += nullable.Value;
                valid = true;
            }

            return valid ? total : null;
        }

        public static Vector3? MultVectorNullables(IEnumerable<Vector3?> vectors)
        {
            bool valid = false;
            Vector3 total = Vector3.one;

            foreach (Vector3? nullable in vectors)
            {
                if (!nullable.HasValue)
                {
                    continue;
                }

                total = Vector3.Scale(total, nullable.Value);
                valid = true;
            }

            return valid ? total : null;
        }

        public static Quaternion? MultQuaternionNullables(IEnumerable<Quaternion?> quaternions)
        {
            bool valid = false;
            Quaternion total = Quaternion.identity;

            foreach (Quaternion? nullable in quaternions)
            {
                if (!nullable.HasValue)
                {
                    continue;
                }

                total *= nullable.Value;
                valid = true;
            }

            return valid ? total : null;
        }

        public static float? MultFloatNullables(IEnumerable<float?> floats)
        {
            bool valid = false;
            float total = 1;

            foreach (float? nullable in floats)
            {
                if (!nullable.HasValue)
                {
                    continue;
                }

                total *= nullable.Value;
                valid = true;
            }

            return valid ? total : null;
        }

        public static Vector4? MultVector4Nullables(IEnumerable<Vector4?> vectors)
        {
            bool valid = false;
            Vector4 total = Vector4.one;

            foreach (Vector4? nullable in vectors)
            {
                if (!nullable.HasValue)
                {
                    continue;
                }

                total = Vector4.Scale(total, nullable.Value);
                valid = true;
            }

            return valid ? total : null;
        }

        public static void MirrorVectorNullable(ref Vector3? vector)
        {
            if (!vector.HasValue)
            {
                return;
            }

            Vector3 modifiedVector = vector.Value;
            modifiedVector.x *= -1;
            vector = modifiedVector;
        }

        public static void MirrorQuaternionNullable(ref Quaternion? quaternion)
        {
            if (!quaternion.HasValue)
            {
                return;
            }

            Quaternion modifiedVector = quaternion.Value;
            quaternion = new Quaternion(modifiedVector.x, modifiedVector.y * -1, modifiedVector.z * -1, modifiedVector.w);
        }
    }
}
