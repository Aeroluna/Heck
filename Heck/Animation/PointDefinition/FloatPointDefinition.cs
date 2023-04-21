using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Heck.Animation
{
    public class FloatPointDefinition : PointDefinition<float>
    {
        internal FloatPointDefinition(IReadOnlyCollection<object> list)
            : base(list)
        {
        }

        protected override bool Compare(float val1, float val2) => val1.EqualsTo(val2);

        protected override float InterpolatePoints(List<IPointData> points, int l, int r, float time)
        {
            return Mathf.LerpUnclamped(points[l].Point, points[r].Point, time);
        }

        private protected override Modifier<float> CreateModifier(float[] floats, Modifier<float>[] modifiers, Operation operation)
        {
            return new Modifier(floats[0], modifiers, operation);
        }

        private protected override IPointData CreatePointData(float[] floats, string[] flags, Modifier<float>[] modifiers, Functions easing)
        {
            return new PointData(floats[0], floats[1], modifiers, easing);
        }

        private class PointData : Modifier, IPointData
        {
            internal PointData(float point, float time, Modifier<float>[] modifiers, Functions easing)
                : base(point, modifiers, default)
            {
                Time = time;
                Easing = easing;
            }

            public float Time { get; }

            public Functions Easing { get; }
        }

        private class Modifier : Modifier<float>
        {
            internal Modifier(float point, Modifier<float>[] modifiers, Operation operation)
                : base(point, modifiers, operation)
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
                        Operation.opMult => current * modifier.Point,
                        Operation.opDivide => current / modifier.Point,
                        _ => throw new InvalidOperationException($"[{modifier.Operation}] cannot be performed on type float.")
                    });
                }
            }

            protected override string FormattedValue => OriginalPoint.ToString(CultureInfo.InvariantCulture);
        }
    }
}
