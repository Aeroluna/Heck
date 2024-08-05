using JetBrains.Annotations;
using UnityEngine;

namespace Heck;

public static class MirrorExtensions
{
    [Pure]
    public static float Mirror(this float @float, bool mirror)
    {
        return mirror ? -@float : @float;
    }

    [Pure]
    public static Quaternion Mirror(this Quaternion quaternion, bool mirror)
    {
        return mirror ? quaternion.Mirror() : quaternion;
    }

    [Pure]
    public static Vector3 Mirror(this Vector3 vector, bool mirror)
    {
        return mirror ? vector.Mirror() : vector;
    }

    [Pure]
    public static Quaternion Mirror(this Quaternion quaternion)
    {
        return new Quaternion(quaternion.x, quaternion.y * -1, quaternion.z * -1, quaternion.w);
    }

    [Pure]
    public static Vector3 Mirror(this Vector3 vector)
    {
        vector.x *= -1;
        return vector;
    }

    [Pure]
    public static float MirrorLineIndex(this float index)
    {
        return -index - 1;
    }
}
