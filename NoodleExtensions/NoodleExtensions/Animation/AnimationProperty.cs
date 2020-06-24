using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NoodleExtensions.Animation
{
    public enum PropertyType
    {
        Vector3,
        Quaternion,
        Linear
    }

    public class Property {
        public PropertyType _propertyType;
        public Coroutine _coroutine;
        public object _property;

        public Property(PropertyType propertyType)
        {
            _propertyType = propertyType;
        }
    }
}
