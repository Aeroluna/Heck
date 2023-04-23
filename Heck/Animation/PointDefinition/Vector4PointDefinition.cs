using System;
using System.Collections.Generic;
using System.Linq;
using Heck.BaseProvider;
using ModestTree;
using UnityEngine;

namespace Heck.Animation
{
    public class Vector4PointDefinition : PointDefinition<Vector4>
    {
        private const int ARRAY_COUNT = 4;

        internal Vector4PointDefinition(IReadOnlyCollection<object> points)
            : base(points)
        {
        }

        internal static Vector3 DivideByComponent(Vector4 val1, Vector4 val2)
        {
            return new Vector3(val1.x / val2.x, val1.y / val2.y, val1.z / val2.z);
        }

        protected override bool Compare(Vector4 val1, Vector4 val2) => val1.EqualsTo(val2);

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
            Color lerped = Color.HSVToRGB(Mathf.LerpUnclamped(hl, hr, time), Mathf.LerpUnclamped(sl, sr, time), Mathf.LerpUnclamped(vl, vr, time));
            return new Vector4(lerped.r, lerped.g, lerped.b, Mathf.LerpUnclamped(pointL.w, pointR.w, time));
        }

        private protected override Modifier<Vector4> CreateModifier(float[]? floats, BaseProviderData? baseProvider, Modifier<Vector4>[] modifiers, Operation operation)
        {
            Vector4? value;
            if (baseProvider != null)
            {
                Assert.That(floats == null);
                value = null;
            }
            else
            {
                Assert.That(floats is { Length: ARRAY_COUNT });
                value = new Vector4(floats![0], floats[1], floats[2], floats[3]);
            }

            return new Modifier(value, baseProvider, modifiers, operation);
        }

        private protected override IPointData CreatePointData(float[] floats, BaseProviderData? baseProvider, string[] flags, Modifier<Vector4>[] modifiers, Functions easing)
        {
            Vector4? value;
            float time;
            if (baseProvider != null)
            {
                Assert.That(floats.Length == 1);
                value = null;
                time = floats[0];
            }
            else
            {
                Assert.That(floats.Length == ARRAY_COUNT + 1);
                value = new Vector4(floats[0], floats[1], floats[2], floats[3]);
                time = floats[ARRAY_COUNT];
            }

            return new PointData(value, baseProvider, flags.Any(n => n == "lerpHSV"), time, modifiers, easing);
        }

        private class PointData : Modifier, IPointData
        {
            internal PointData(Vector4? point, BaseProviderData? baseProvider, bool hsvLerp, float time, Modifier<Vector4>[] modifiers, Functions easing)
                : base(point, baseProvider, modifiers, default)
            {
                HsvLerp = hsvLerp;
                Time = time;
                Easing = easing;
            }

            public bool HsvLerp { get; }

            public float Time { get; }

            public Functions Easing { get; }
        }

        private class Modifier : Modifier<Vector4>
        {
            internal Modifier(Vector4? point, BaseProviderData? baseProvider, Modifier<Vector4>[] modifiers, Operation operation)
                : base(point, baseProvider, modifiers, operation)
            {
            }

            public override Vector4 Point
            {
                get
                {
                    return Modifiers.Aggregate(OriginalPoint, (current, modifier) => modifier.Operation switch
                    {
                        Operation.opAdd => current + modifier.Point,
                        Operation.opSub => current - modifier.Point,
                        Operation.opMul => Vector4.Scale(current, modifier.Point),
                        Operation.opDiv => DivideByComponent(current, modifier.Point),
                        _ => throw new InvalidOperationException($"[{modifier.Operation}] cannot be performed on type Vector3.")
                    });
                }
            }

            protected override string FormattedValue => $"{OriginalPoint.x}, {OriginalPoint.y}, {OriginalPoint.z}, {OriginalPoint.w}";
        }
    }
}
