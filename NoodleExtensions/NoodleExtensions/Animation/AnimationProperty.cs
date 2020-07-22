namespace NoodleExtensions.Animation
{
    using UnityEngine;

    public enum PropertyType
    {
        Vector3,
        Vector4,
        Quaternion,
        Linear,
    }

    public class Property
    {
        public Property(PropertyType propertyType)
        {
            PropertyType = propertyType;
        }

        public PropertyType PropertyType { get; }

        public Coroutine Coroutine { get; set; }

        public object Value { get; set; }
    }
}
