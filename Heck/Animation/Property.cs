namespace Heck.Animation
{
    using UnityEngine;

    public enum PropertyType
    {
        Vector3,
        Vector4,
        Quaternion,
        Linear,
    }

    internal class PathProperty : Property
    {
        internal PathProperty(PropertyType propertyType)
            : base(propertyType)
        {
        }

        internal PointDefinitionInterpolation Interpolation { get; } = new PointDefinitionInterpolation();

        internal override object? Value => Interpolation;
    }

    internal class Property
    {
        internal Property(PropertyType propertyType)
        {
            PropertyType = propertyType;
        }

        internal PropertyType PropertyType { get; }

        internal Coroutine? Coroutine { get; set; }

        internal virtual object? Value { get; set; }
    }
}
