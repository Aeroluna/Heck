using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Heck.BaseProvider;
using ModestTree;
using UnityEngine;

namespace Heck.Animation
{
    public class FloatPointDefinition : PointDefinition<float>
    {
        private const int ARRAY_COUNT = 1;

        internal FloatPointDefinition(IReadOnlyCollection<object> list)
            : base(list)
        {
        }

        protected override float InterpolatePoints(List<IPointData> points, int l, int r, float time)
        {
            return Mathf.LerpUnclamped(points[l].Point, points[r].Point, time);
        }

        private protected override Modifier<float> CreateModifier(float[]? floats, BaseProviderData? baseProvider, Modifier<float>[] modifiers, Operation operation)
        {
            if (baseProvider != null)
            {
                Assert.IsNull(floats, "Modifier cannot have both base and a point");
            }
            else
            {
                Assert.IsNotNull(floats, "Modifier without base must have a point");
                Assert.IsEqual(ARRAY_COUNT, floats!.Length, $"Float modifier point must have {ARRAY_COUNT} numbers");
            }

            return new Modifier(floats?[0], baseProvider, modifiers, operation);
        }

        private protected override IPointData CreatePointData(float[] floats, BaseProviderData? baseProvider, string[] flags, Modifier<float>[] modifiers, Functions easing)
        {
            float? value;
            float time;
            if (baseProvider != null)
            {
                Assert.IsEqual(1, floats.Length, "Point with base must have only time");
                value = null;
                time = floats[0];
            }
            else
            {
                Assert.IsEqual(ARRAY_COUNT + 1, floats.Length, $"Float point must have {ARRAY_COUNT + 1} numbers");
                value = floats[0];
                time = floats[ARRAY_COUNT];
            }

            return new PointData(value, baseProvider, time, modifiers, easing);
        }

        private class PointData : Modifier, IPointData
        {
            internal PointData(float? point, BaseProviderData? baseProvider, float time, Modifier<float>[] modifiers, Functions easing)
                : base(point, baseProvider, modifiers, default)
            {
                Time = time;
                Easing = easing;
            }

            public float Time { get; }

            public Functions Easing { get; }
        }

        private class Modifier : Modifier<float>
        {
            internal Modifier(float? point, BaseProviderData? baseProvider, Modifier<float>[] modifiers, Operation operation)
                : base(point, baseProvider, modifiers, operation)
            {
            }

            public override float Point
            {
                get
                {
                    return Modifiers.Aggregate(OriginalPoint, (current, modifier) => modifier.Operation switch
                    {
                        Operation.opAdd => current + modifier.Point,
                        Operation.opSub => current - modifier.Point,
                        Operation.opMul => current * modifier.Point,
                        Operation.opDiv => current / modifier.Point,
                        _ => throw new InvalidOperationException($"[{modifier.Operation}] cannot be performed on type float.")
                    });
                }
            }

            protected override string FormattedValue => OriginalPoint.ToString(CultureInfo.InvariantCulture);
        }
    }
}
