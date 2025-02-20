using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;

namespace Heck.Animation;

public class Vector4PointDefinition : PointDefinition<Vector4>
{
    private const int ARRAY_SIZE = 4;

    internal Vector4PointDefinition(IReadOnlyCollection<object> points)
        : base(points)
    {
    }

    internal static Vector3 DivideByComponent(Vector4 val1, Vector4 val2)
    {
        return new Vector3(val1.x / val2.x, val1.y / val2.y, val1.z / val2.z);
    }

    protected override Vector4 InterpolatePoints(List<IPointData> points, int l, int r, float time)
    {
        PointData pointRData = (PointData)points[r];
        Vector4 pointL = points[l].Point;
        Vector4 pointR = pointRData.Point;
        if (!pointRData.HsvLerp)
        {
            return Vector4.LerpUnclamped(pointL, pointR, time);
        }

        Color.RGBToHSV(pointL, out float hl, out float sl, out float vl);
        Color.RGBToHSV(pointR, out float hr, out float sr, out float vr);
        Color lerped = Color.HSVToRGB(
            Mathf.LerpUnclamped(hl, hr, time),
            Mathf.LerpUnclamped(sl, sr, time),
            Mathf.LerpUnclamped(vl, vr, time));
        return new Vector4(lerped.r, lerped.g, lerped.b, Mathf.LerpUnclamped(pointL.w, pointR.w, time));
    }

    private protected override Modifier<Vector4> CreateModifier(
        IValues[] values,
        Modifier<Vector4>[] modifiers,
        Operation operation)
    {
        Vector4? value;
        IValues[]? result;
        if (values.Length == 1 && values[0] is StaticValues { Values.Length: ARRAY_SIZE } staticValues)
        {
            float[] valueValues = staticValues.Values;
            value = new Vector4(valueValues[0], valueValues[1], valueValues[2], valueValues[3]);
            result = null;
        }
        else
        {
            value = null;
            result = values;
            Assert.IsEqual(ARRAY_SIZE, result.Sum(n => n.Values.Length), $"Vector4 modifier point must have {ARRAY_SIZE} numbers");
        }

        return new Modifier(value, result, modifiers, operation);
    }

    private protected override IPointData CreatePointData(
        IValues[] values,
        string[] flags,
        Modifier<Vector4>[] modifiers,
        Functions easing)
    {
        Vector4? value;
        float time;
        IValues[]? result;
        if (values.Length == 1 && values[0] is StaticValues { Values.Length: ARRAY_SIZE + 1 } staticValues)
        {
            float[] valueValues = staticValues.Values;
            value = new Vector4(valueValues[0], valueValues[1], valueValues[2], valueValues[3]);
            result = null;
            time = valueValues[ARRAY_SIZE];
        }
        else
        {
            value = null;
            result = values;
            Assert.IsEqual(ARRAY_SIZE + 1, result.Sum(n => n.Values.Length), $"Vector4 modifier point must have {ARRAY_SIZE + 1} numbers");
            float[] last = result[result.Length - 1].Values;
            time = last[last.Length - 1];
        }

        return new PointData(value, result, flags.Any(n => n == "lerpHSV"), time, modifiers, easing);
    }

    private class PointData : Modifier, IPointData
    {
        internal PointData(
            Vector4? point,
            IValues[]? values,
            bool hsvLerp,
            float time,
            Modifier<Vector4>[] modifiers,
            Functions easing)
            : base(point, values, modifiers, default)
        {
            HsvLerp = hsvLerp;
            Time = time;
            Easing = easing;
        }

        public Functions Easing { get; }

        public bool HsvLerp { get; }

        public float Time { get; }
    }

    private class Modifier : Modifier<Vector4>
    {
        internal Modifier(
            Vector4? point,
            IValues[]? values,
            Modifier<Vector4>[] modifiers,
            Operation operation)
            : base(point, values, modifiers, operation, ARRAY_SIZE)
        {
        }

        public override Vector4 Point
        {
            get
            {
                return Modifiers.Aggregate(
                    OriginalPoint,
                    (current, modifier) => modifier.Operation switch
                    {
                        Operation.opAdd => current + modifier.Point,
                        Operation.opSub => current - modifier.Point,
                        Operation.opMul => Vector4.Scale(current, modifier.Point),
                        Operation.opDiv => DivideByComponent(current, modifier.Point),
                        _ => throw new InvalidOperationException(
                            $"[{modifier.Operation}] cannot be performed on type Vector4.")
                    });
            }
        }

        protected override string FormattedValue =>
            $"{OriginalPoint.x}, {OriginalPoint.y}, {OriginalPoint.z}, {OriginalPoint.w}";

        protected override Vector4 Translate(float[] array)
        {
            return new Vector4(array[0], array[1], array[2], array[3]);
        }
    }
}
