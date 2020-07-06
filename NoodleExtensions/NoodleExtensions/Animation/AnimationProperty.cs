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

        public PropertyType PropertyType { get; set; }

        public Coroutine Coroutine { get; set; }

        public object Value { get; set; }
    }
}
