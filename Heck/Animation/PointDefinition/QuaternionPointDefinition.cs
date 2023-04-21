using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heck.Animation
{
    public class QuaternionPointDefinition : PointDefinition<Quaternion>
    {
        internal QuaternionPointDefinition(IReadOnlyCollection<object> list)
            : base(list)
        {
        }

        protected override bool Compare(Quaternion val1, Quaternion val2) => val1.EqualsTo(val2);

        protected override Quaternion InterpolatePoints(List<IPointData> points, int l, int r, float time)
        {
            return Quaternion.SlerpUnclamped(points[l].Point, points[r].Point, time);
        }

        private protected override Modifier<Quaternion> CreateModifier(float[] floats, Modifier<Quaternion>[] modifiers, Operation operation)
        {
            Quaternion value = Quaternion.Euler(new Vector3(floats[0], floats[1], floats[2]));
            return new Modifier(value, modifiers, operation);
        }

        private protected override IPointData CreatePointData(float[] floats, string[] flags, Modifier<Quaternion>[] modifiers, Functions easing)
        {
            Quaternion value = Quaternion.Euler(new Vector3(floats[0], floats[1], floats[2]));
            return new PointData(value, floats[3], modifiers, easing);
        }

        private class PointData : Modifier, IPointData
        {
            internal PointData(Quaternion point, float time, Modifier<Quaternion>[] modifiers, Functions easing)
                : base(point, modifiers, default)
            {
                Time = time;
                Easing = easing;
            }

            public float Time { get; }

            public Functions Easing { get; }
        }

        private class Modifier : Modifier<Quaternion>
        {
            internal Modifier(Quaternion point, Modifier<Quaternion>[] modifiers, Operation operation)
                : base(point, modifiers, operation)
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
