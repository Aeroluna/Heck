using UnityEngine;

namespace Heck
{
    public static class MirrorExtensions
    {
        public static float Mirror(this float @float, bool mirror)
        {
            return mirror ? -@float : @float;
        }

        public static Quaternion Mirror(this Quaternion quaternion, bool mirror)
        {
            return mirror ? quaternion.Mirror() : quaternion;
        }

        public static Vector3 Mirror(this Vector3 vector, bool mirror)
        {
            return mirror ? vector.Mirror() : vector;
        }

        public static Quaternion Mirror(this Quaternion quaternion)
        {
            return new Quaternion(quaternion.x, quaternion.y * -1, quaternion.z * -1, quaternion.w);
        }

        public static Vector3 Mirror(this Vector3 vector)
        {
            vector.x *= -1;
            return vector;
        }
    }
}
