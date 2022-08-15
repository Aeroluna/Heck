using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using UnityEngine;

namespace Heck.Animation
{
    internal interface IPropertyBuilder
    {
        public BaseProperty Property { get; }

        public BaseProperty PathProperty { get; }

        public IPointDefinition? GetPointData(CustomData customData, string pointName, Dictionary<string, List<object>> pointDefinitions);
    }

    internal abstract class BaseProperty
    {
        internal Coroutine? Coroutine { get; set; }

        internal abstract void Null();
    }

    internal abstract class BasePathProperty : BaseProperty
    {
        internal abstract IPointDefinitionInterpolation IInterpolation { get; }
    }

    internal class PathProperty<T> : BasePathProperty
        where T : struct
    {
        internal override IPointDefinitionInterpolation IInterpolation => Interpolation;

        internal PointDefinitionInterpolation<T> Interpolation { get; } = new();

        internal override void Null() => Interpolation.Init(null);
    }

    internal class Property<T> : BaseProperty
        where T : struct
    {
        internal T? Value { get; set; }

        internal override void Null() => Value = null;
    }

    internal class PropertyBuilder<T> : IPropertyBuilder
        where T : struct
    {
        public BaseProperty Property => new Property<T>();

        public BaseProperty PathProperty => new PathProperty<T>();

        public IPointDefinition? GetPointData(CustomData customData, string pointName, Dictionary<string, List<object>> pointDefinitions)
        {
            return customData.GetPointData<T>(pointName, pointDefinitions);
        }
    }
}
