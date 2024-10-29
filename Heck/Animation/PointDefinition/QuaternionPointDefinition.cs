using System;
using System.Collections.Generic;
using System.Linq;
using Heck.BaseProvider;
using ModestTree;
using UnityEngine;

namespace Heck.Animation;

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

    private protected override Modifier<Quaternion> CreateModifier(
        float[]? floats,
        BaseProviderData<Quaternion>? baseProvider,
        Modifier<Quaternion>[] modifiers,
        Operation operation)
    {
        Quaternion? value;
        Vector3? vectorPoint;
        if (baseProvider != null)
        {
            Assert.IsNull(floats, "Modifier cannot have both base and a point");
            vectorPoint = null;
            value = null;
        }
        else
        {
            Assert.IsNotNull(floats, "Modifier without base must have a point");
            Assert.IsEqual(ARRAY_COUNT, floats!.Length, $"Quaternion modifier point must have {ARRAY_COUNT} numbers");
            Vector3 vector = new(floats[0], floats[1], floats[2]);
            vectorPoint = vector;
            value = Quaternion.Euler(vector);
        }

        return new Modifier(value, vectorPoint, baseProvider, modifiers, operation);
    }

    private protected override IPointData CreatePointData(
        float[] floats,
        BaseProviderData<Quaternion>? baseProvider,
        string[] flags,
        Modifier<Quaternion>[] modifiers,
        Functions easing)
    {
        Quaternion? value;
        Vector3? vectorPoint;
        float time;
        if (baseProvider != null)
        {
            Assert.IsEqual(1, floats.Length, "Point with base must have only time");
            vectorPoint = null;
            value = null;
            time = floats[0];
        }
        else
        {
            Assert.IsEqual(ARRAY_COUNT + 1, floats.Length, $"Quaternion point must have {ARRAY_COUNT + 1} numbers");
            Vector3 vector = new(floats[0], floats[1], floats[2]);
            vectorPoint = vector;
            value = Quaternion.Euler(vector);
            time = floats[ARRAY_COUNT];
        }

        return new PointData(value, vectorPoint, baseProvider, time, modifiers, easing);
    }

    private class PointData : Modifier, IPointData
    {
        internal PointData(
            Quaternion? point,
            Vector3? vectorPoint,
            BaseProviderData<Quaternion>? baseProvider,
            float time,
            Modifier<Quaternion>[] modifiers,
            Functions easing)
            : base(point, vectorPoint, baseProvider, modifiers, default)
        {
            Time = time;
            Easing = easing;
        }

        public Functions Easing { get; }

        public float Time { get; }
    }

    private class Modifier : Modifier<Quaternion>
    {
        private readonly BaseProviderData<Quaternion>? _baseProvider;
        private readonly Vector3? _vectorPoint;

        internal Modifier(
            Quaternion? point,
            Vector3? vectorPoint,
            BaseProviderData<Quaternion>? baseProvider,
            Modifier<Quaternion>[] modifiers,
            Operation operation)
            : base(point, baseProvider, modifiers, operation)
        {
            _vectorPoint = vectorPoint;
            _baseProvider = baseProvider;
        }

        public override Quaternion Point => Modifiers.Length == 0 ? OriginalPoint : Quaternion.Euler(VectorPoint);

        protected override string FormattedValue => Vector3PointDefinition.Format(OriginalPoint.eulerAngles);

        private Vector3 VectorPoint
        {
            get
            {
                Vector3 original = _vectorPoint ??
                                   _baseProvider?.GetValue().eulerAngles ?? throw new InvalidOperationException();
                return Modifiers.Aggregate(
                    original,
                    (current, genericModifier) =>
                    {
                        Modifier modifier = (Modifier)genericModifier;
                        Vector3 modVec = modifier.VectorPoint;
                        Vector3 resVec = modifier.Operation switch
                        {
                            Operation.opAdd => current + modVec,
                            Operation.opSub => current - modVec,
                            Operation.opMul => Vector3.Scale(current, modVec),
                            Operation.opDiv => Vector3PointDefinition.DivideByComponent(current, modVec),
                            _ => throw new InvalidOperationException(
                                $"[{modifier.Operation}] cannot be performed on type Quaternion.")
                        };

                        return resVec;
                    });
            }
        }
    }
}
