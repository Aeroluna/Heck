using System;
using System.Collections.Generic;
using System.Linq;
using Heck.BaseProvider;
using ModestTree;
using UnityEngine;

namespace Heck.Animation
{
    public class QuaternionPointDefinition : PointDefinition<Quaternion>
    {
        private const int ARRAY_COUNT = 3;

        internal QuaternionPointDefinition(IReadOnlyCollection<object> list)
            : base(list)
        {
        }

        protected override bool Compare(Quaternion val1, Quaternion val2) => val1.EqualsTo(val2);

        protected override Quaternion InterpolatePoints(List<IPointData> points, int l, int r, float time)
        {
            return Quaternion.SlerpUnclamped(points[l].Point, points[r].Point, time);
        }

        private protected override Modifier<Quaternion> CreateModifier(float[]? floats, BaseProviderData? baseProvider, Modifier<Quaternion>[] modifiers, Operation operation)
        {
            Quaternion? value;
            if (baseProvider != null)
            {
                Assert.That(floats == null);
                value = null;
            }
            else
            {
                Assert.That(floats is { Length: ARRAY_COUNT });
                value = Quaternion.Euler(new Vector3(floats![0], floats[1], floats[2]));
            }

            return new Modifier(value, baseProvider, modifiers, operation);
        }

        private protected override IPointData CreatePointData(float[] floats, BaseProviderData? baseProvider, string[] flags, Modifier<Quaternion>[] modifiers, Functions easing)
        {
            Quaternion? value;
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
                value = Quaternion.Euler(new Vector3(floats[0], floats[1], floats[2]));
                time = floats[ARRAY_COUNT];
            }

            return new PointData(value, baseProvider, time, modifiers, easing);
        }

        private class PointData : Modifier, IPointData
        {
            internal PointData(Quaternion? point, BaseProviderData? baseProvider, float time, Modifier<Quaternion>[] modifiers, Functions easing)
                : base(point, baseProvider, modifiers, default)
            {
                Time = time;
                Easing = easing;
            }

            public float Time { get; }

            public Functions Easing { get; }
        }

        private class Modifier : Modifier<Quaternion>
        {
            internal Modifier(Quaternion? point, BaseProviderData? baseProvider, Modifier<Quaternion>[] modifiers, Operation operation)
                : base(point, baseProvider, modifiers, operation)
            {
            }

            public override Quaternion Point
            {
                get
                {
                    return Modifiers.Aggregate(OriginalPoint, (current, modifier) =>
                    {
                        // Potentially painfully slow by converting between vectors and quaternions
                        Vector3 curVec = current.eulerAngles;
                        Vector3 modVec = modifier.Point.eulerAngles;
                        Vector3 resVec = modifier.Operation switch
                        {
                            Operation.opAdd => curVec + curVec,
                            Operation.opSub => modVec - modVec,
                            Operation.opMult => Vector3.Scale(curVec, modVec),
                            Operation.opDivide => Vector3PointDefinition.DivideByComponent(curVec, modVec),
                            _ => throw new InvalidOperationException($"[{modifier.Operation}] cannot be performed on type float.")
                        };

                        return Quaternion.Euler(resVec);
                    });
                }
            }

            protected override string FormattedValue => Vector3PointDefinition.Format(OriginalPoint.eulerAngles);
        }
    }
}
