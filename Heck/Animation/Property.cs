using UnityEngine;

namespace Heck.Animation
{
    public enum PropertyType
    {
        Vector3,
        Vector4,
        Quaternion,
        Linear
    }

    internal class PathProperty : Property
    {
        internal PathProperty(PropertyType propertyType)
            : base(propertyType)
        {
        }

        internal PointDefinitionInterpolation Interpolation { get; } = new();
    }

    internal class Property
    {
        internal Property(PropertyType propertyType)
        {
            PropertyType = propertyType;
        }

        internal PropertyType PropertyType { get; }

        internal Coroutine? Coroutine { get; set; }

        // avoid boxing
        internal float? LinearValue { get; set; }

        internal Vector3? Vector3Value { get; set; }

        internal Vector4? Vector4Value { get; set; }

        internal Quaternion? QuaternionValue { get; set; }
    }
}
