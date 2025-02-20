using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;

namespace Heck.Animation;

public class Vector3PointDefinition : PointDefinition<Vector3>
{
    private const int ARRAY_SIZE = 3;

    internal Vector3PointDefinition(IReadOnlyCollection<object> points)
        : base(points)
    {
    }

    internal static Vector3 DivideByComponent(Vector3 val1, Vector3 val2)
    {
        return new Vector3(val1.x / val2.x, val1.y / val2.y, val1.z / val2.z);
    }

    internal static string Format(Vector3 input)
    {
        return $"{input.x}, {input.y}, {input.z}";
    }

    protected override Vector3 InterpolatePoints(List<IPointData> points, int l, int r, float time)
    {
        PointData pointR = (PointData)points[r];
        return pointR.Smooth
            ? SmoothVectorLerp(points, l, r, time)
            : Vector3.LerpUnclamped(points[l].Point, pointR.Point, time);
    }

    private protected override Modifier<Vector3> CreateModifier(
        IValues[] values,
        Modifier<Vector3>[] modifiers,
        Operation operation)
    {
        Vector3? value;
        IValues[]? result;
        if (values.Length == 1 && values[0] is StaticValues { Values.Length: ARRAY_SIZE } staticValues)
        {
            float[] valueValues = staticValues.Values;
            value = new Vector3(valueValues[0], valueValues[1], valueValues[2]);
            result = null;
        }
        else
        {
            value = null;
            result = values;
            Assert.IsEqual(ARRAY_SIZE, result.Sum(n => n.Values.Length), $"Vector3 modifier point must have {ARRAY_SIZE} numbers");
        }

        return new Modifier(value, result, modifiers, operation);
    }

    private protected override IPointData CreatePointData(
        IValues[] values,
        string[] flags,
        Modifier<Vector3>[] modifiers,
        Functions easing)
    {
        Vector3? value;
        float time;
        IValues[]? result;
        if (values.Length == 1 && values[0] is StaticValues { Values.Length: ARRAY_SIZE + 1 } staticValues)
        {
            float[] valueValues = staticValues.Values;
            value = new Vector3(valueValues[0], valueValues[1], valueValues[2]);
            result = null;
            time = valueValues[ARRAY_SIZE];
        }
        else
        {
            value = null;
            result = values;
            Assert.IsEqual(ARRAY_SIZE + 1, result.Sum(n => n.Values.Length), $"Vector3 modifier point must have {ARRAY_SIZE + 1} numbers");
            float[] last = result[result.Length - 1].Values;
            time = last[last.Length - 1];
        }

        return new PointData(
            value,
            result,
            flags.Any(n => n == "splineCatmullRom"),
            time,
            modifiers,
            easing); // TODO: add more spicy splines
    }

    private static Vector3 SmoothVectorLerp(List<IPointData> points, int a, int b, float time)
    {
        Vector3 pa = points[a].Point;
        Vector3 pb = points[b].Point;

        // Catmull-Rom Spline
        Vector3 p0 = a - 1 < 0 ? pa : points[a - 1].Point;
        ////Vector3 p1 = pa;
        ////Vector3 p2 = pb;
        Vector3 p3 = b + 1 > points.Count - 1 ? pb : points[b + 1].Point;

        float tt = time * time;
        float ttt = tt * time;

        float q0 = (-ttt + (2.0f * tt)) - time;
        float q1 = ((3.0f * ttt) - (5.0f * tt)) + 2.0f;
        float q2 = (-3.0f * ttt) + (4.0f * tt) + time;
        float q3 = ttt - tt;

        Vector3 c = 0.5f * ((p0 * q0) + (pa * q1) + (pb * q2) + (p3 * q3));

        return c;
    }

    private class PointData : Modifier, IPointData
    {
        internal PointData(
            Vector3? point,
            IValues[]? values,
            bool smooth,
            float time,
            Modifier<Vector3>[] modifiers,
            Functions easing)
            : base(point, values, modifiers, default)
        {
            Smooth = smooth;
            Time = time;
            Easing = easing;
        }

        public Functions Easing { get; }

        public bool Smooth { get; }

        public float Time { get; }
    }

    private class Modifier : Modifier<Vector3>
    {
        internal Modifier(
            Vector3? point,
            IValues[]? values,
            Modifier<Vector3>[] modifiers,
            Operation operation)
            : base(point, values, modifiers, operation, ARRAY_SIZE)
        {
        }

        public override Vector3 Point
        {
            get
            {
                return Modifiers.Aggregate(
                    OriginalPoint,
                    (current, modifier) => modifier.Operation switch
                    {
                        Operation.opAdd => current + modifier.Point,
                        Operation.opSub => current - modifier.Point,
                        Operation.opMul => Vector3.Scale(current, modifier.Point),
                        Operation.opDiv => DivideByComponent(current, modifier.Point),
                        _ => throw new InvalidOperationException(
                            $"[{modifier.Operation}] cannot be performed on type Vector3.")
                    });
            }
        }

        protected override string FormattedValue => Format(OriginalPoint);

        protected override Vector3 Translate(float[] array)
        {
            return new Vector3(array[0], array[1], array[2]);
        }
    }
}
