using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ModestTree;
using UnityEngine;

namespace Heck.Animation;

public class FloatPointDefinition : PointDefinition<float>
{
    private const int ARRAY_SIZE = 1;

    internal FloatPointDefinition(IReadOnlyCollection<object> list)
        : base(list)
    {
    }

    protected override float InterpolatePoints(List<IPointData> points, int l, int r, float time)
    {
        return Mathf.LerpUnclamped(points[l].Point, points[r].Point, time);
    }

    private protected override Modifier<float> CreateModifier(
        IValues[] values,
        Modifier<float>[] modifiers,
        Operation operation)
    {
        float? value;
        IValues[]? result;
        if (values.Length == 1 && values[0] is StaticValues { Values.Length: ARRAY_SIZE } staticValues)
        {
            value = staticValues.Values[0];
            result = null;
        }
        else
        {
            value = null;
            result = values;
            Assert.IsEqual(ARRAY_SIZE, result.Sum(n => n.Values.Length), $"Float modifier point must have {ARRAY_SIZE} numbers");
        }

        return new Modifier(value, result, modifiers, operation);
    }

    private protected override IPointData CreatePointData(
        IValues[] values,
        string[] flags,
        Modifier<float>[] modifiers,
        Functions easing)
    {
        float? value;
        float time;
        IValues[]? result;
        if (values.Length == 1 && values[0] is StaticValues { Values.Length: ARRAY_SIZE + 1 } staticValues)
        {
            value = staticValues.Values[0];
            result = null;
            time = staticValues.Values[ARRAY_SIZE];
        }
        else
        {
            value = null;
            result = values;
            Assert.IsEqual(ARRAY_SIZE + 1, result.Sum(n => n.Values.Length), $"Float modifier point must have {ARRAY_SIZE + 1} numbers");
            float[] last = result[result.Length - 1].Values;
            time = last[last.Length - 1];
        }

        return new PointData(value, result, time, modifiers, easing);
    }

    private class PointData : Modifier, IPointData
    {
        internal PointData(
            float? point,
            IValues[]? values,
            float time,
            Modifier<float>[] modifiers,
            Functions easing)
            : base(point, values, modifiers, default)
        {
            Time = time;
            Easing = easing;
        }

        public Functions Easing { get; }

        public float Time { get; }
    }

    private class Modifier : Modifier<float>
    {
        internal Modifier(
            float? point,
            IValues[]? values,
            Modifier<float>[] modifiers,
            Operation operation)
            : base(point, values, modifiers, operation, ARRAY_SIZE)
        {
        }

        public override float Point
        {
            get
            {
                return Modifiers.Aggregate(
                    OriginalPoint,
                    (current, modifier) => modifier.Operation switch
                    {
                        Operation.opAdd => current + modifier.Point,
                        Operation.opSub => current - modifier.Point,
                        Operation.opMul => current * modifier.Point,
                        Operation.opDiv => current / modifier.Point,
                        _ => throw new InvalidOperationException(
                            $"[{modifier.Operation}] cannot be performed on type float.")
                    });
            }
        }

        protected override string FormattedValue => OriginalPoint.ToString(CultureInfo.InvariantCulture);

        protected override float Translate(float[] array)
        {
            return array[0];
        }
    }
}
