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

        protected override Quaternion InterpolatePoints(List<IPointData> points, int l, int r, float time)
        {
            return Quaternion.SlerpUnclamped(points[l].Point, points[r].Point, time);
        }

        private protected override Modifier<Quaternion> CreateModifier(float[]? floats, BaseProviderData<Quaternion>? baseProvider, Modifier<Quaternion>[] modifiers, Operation operation)
        {
            Quaternion? value;
            if (baseProvider != null)
            {
                Assert.IsNull(floats, "Modifier cannot have both base and a point");
                value = null;
            }
            else
            {
                Assert.IsNotNull(floats, "Modifier without base must have a point");
                Assert.IsEqual(ARRAY_COUNT, floats!.Length, $"Quaternion modifier point must have {ARRAY_COUNT} numbers");
                value = Quaternion.Euler(new Vector3(floats[0], floats[1], floats[2]));
            }

            return new Modifier(value, baseProvider, modifiers, operation);
        }

        private protected override IPointData CreatePointData(float[] floats, BaseProviderData<Quaternion>? baseProvider, string[] flags, Modifier<Quaternion>[] modifiers, Functions easing)
        {
            Quaternion? value;
            float time;
            if (baseProvider != null)
            {
                Assert.IsEqual(1, floats.Length, "Point with base must have only time");
                value = null;
                time = floats[0];
            }
            else
            {
                Assert.IsEqual(ARRAY_COUNT + 1, floats.Length, $"Quaternion point must have {ARRAY_COUNT + 1} numbers");
                value = Quaternion.Euler(new Vector3(floats[0], floats[1], floats[2]));
                time = floats[ARRAY_COUNT];
            }

            return new PointData(value, baseProvider, time, modifiers, easing);
        }

        private class PointData : Modifier, IPointData
        {
            internal PointData(Quaternion? point, BaseProviderData<Quaternion>? baseProvider, float time, Modifier<Quaternion>[] modifiers, Functions easing)
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
            internal Modifier(Quaternion? point, BaseProviderData<Quaternion>? baseProvider, Modifier<Quaternion>[] modifiers, Operation operation)
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
                            Operation.opMul => Vector3.Scale(curVec, modVec),
                            Operation.opDiv => Vector3PointDefinition.DivideByComponent(curVec, modVec),
                            _ => throw new InvalidOperationException($"[{modifier.Operation}] cannot be performed on type Quaternion.")
                        };

                        return Quaternion.Euler(resVec);
                    });
                }
            }

            protected override string FormattedValue => Vector3PointDefinition.Format(OriginalPoint.eulerAngles);
        }
    }
}
