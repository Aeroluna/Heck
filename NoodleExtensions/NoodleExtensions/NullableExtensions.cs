namespace NoodleExtensions
{
    using System;
    using UnityEngine;
    using static NoodleExtensions.Plugin;

    public static class NullableExtensions
    {
        public static float? ToNullableFloat(this object @this)
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

            Vector3 total = _vectorZero;
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
                if (vectorTwo.HasValue)
                {
                    return Vector3.Scale(vectorOne.Value, vectorTwo.Value);
                }
                else
                {
                    return vectorOne;
                }
            }
            else if (vectorTwo.HasValue)
            {
                return vectorTwo;
            }

            return null;
        }

        public static Quaternion? MultQuaternionNullables(Quaternion? quaternionOne, Quaternion? quaternionTwo)
        {
            if (quaternionOne.HasValue)
            {
                if (quaternionTwo.HasValue)
                {
                    return quaternionOne.Value * quaternionTwo.Value;
                }
                else
                {
                    return quaternionOne;
                }
            }
            else if (quaternionTwo.HasValue)
            {
                return quaternionTwo;
            }

            return null;
        }

        public static float? MultFloatNullables(float? floatOne, float? floatTwo)
        {
            if (floatOne.HasValue)
            {
                if (floatTwo.HasValue)
                {
                    return floatOne.Value * floatTwo.Value;
                }
                else
                {
                    return floatOne;
                }
            }
            else if (floatTwo.HasValue)
            {
                return floatTwo;
            }

            return null;
        }

        public static Vector4? MultVector4Nullables(Vector4? vectorOne, Vector4? vectorTwo)
        {
            if (vectorOne.HasValue)
            {
                if (vectorTwo.HasValue)
                {
                    return Vector4.Scale(vectorOne.Value, vectorTwo.Value);
                }
                else
                {
                    return vectorOne;
                }
            }
            else if (vectorTwo.HasValue)
            {
                return vectorTwo;
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
