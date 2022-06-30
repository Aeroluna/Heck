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

        internal override object Value => Interpolation;
    }

    internal class Property
    {
        internal Property(PropertyType propertyType)
        {
            PropertyType = propertyType;
        }

        internal PropertyType PropertyType { get; }

        internal Coroutine? Coroutine { get; set; }

        // TODO: Avoid boxing this, could use a Vector4 and PropertyType
        internal virtual object? Value { get; set; }
    }
}
