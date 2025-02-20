using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;

namespace Heck.Animation;

public class QuaternionPointDefinition : PointDefinition<Quaternion>
{
    private const int ARRAY_SIZE = 3;

    internal QuaternionPointDefinition(IReadOnlyCollection<object> list)
        : base(list)
    {
    }

    protected override Quaternion InterpolatePoints(List<IPointData> points, int l, int r, float time)
    {
        return Quaternion.SlerpUnclamped(points[l].Point, points[r].Point, time);
    }

    private protected override Modifier<Quaternion> CreateModifier(
        IValues[] values,
        Modifier<Quaternion>[] modifiers,
        Operation operation)
    {
        Quaternion? value;
        Vector3? vectorPoint;
        IValues[]? result;
        if (values.Length == 1 && values[0] is StaticValues { Values.Length: ARRAY_SIZE } staticValues)
        {
            float[] valueValues = staticValues.Values;
            Vector3 vector = new(valueValues[0], valueValues[1], valueValues[2]);
            vectorPoint = vector;
            value = Quaternion.Euler(vector);
            result = null;
        }
        else
        {
            vectorPoint = null;
            value = null;
            result = values;
            Assert.IsEqual(ARRAY_SIZE, result.Sum(n => n.Values.Length), $"Quaternion modifier point must have {ARRAY_SIZE} numbers");
        }

        return new Modifier(value, vectorPoint, result, modifiers, operation);
    }

    private protected override IPointData CreatePointData(
        IValues[] values,
        string[] flags,
        Modifier<Quaternion>[] modifiers,
        Functions easing)
    {
        Quaternion? value;
        Vector3? vectorPoint;
        float time;
        IValues[]? result;
        if (values.Length == 1 && values[0] is StaticValues { Values.Length: ARRAY_SIZE + 1 } staticValues)
        {
            float[] valueValues = staticValues.Values;
            Vector3 vector = new(valueValues[0], valueValues[1], valueValues[2]);
            vectorPoint = vector;
            value = Quaternion.Euler(vector);
            result = null;
            time = valueValues[ARRAY_SIZE];
        }
        else
        {
            vectorPoint = null;
            value = null;
            result = values;
            Assert.IsEqual(ARRAY_SIZE + 1, result.Sum(n => n.Values.Length), $"Quaternion modifier point must have {ARRAY_SIZE + 1} numbers");
            float[] last = result[result.Length - 1].Values;
            time = last[last.Length - 1];
        }

        return new PointData(value, vectorPoint, result, time, modifiers, easing);
    }

    private class PointData : Modifier, IPointData
    {
        internal PointData(
            Quaternion? point,
            Vector3? vectorPoint,
            IValues[]? values,
            float time,
            Modifier<Quaternion>[] modifiers,
            Functions easing)
            : base(point, vectorPoint, values, modifiers, default)
        {
            Time = time;
            Easing = easing;
        }

        public Functions Easing { get; }

        public float Time { get; }
    }

    private class Modifier : Modifier<Quaternion>
    {
        private readonly float[] _reusableArray = new float[ARRAY_SIZE];

        private readonly IValues[]? _values;
        private readonly Vector3? _vectorPoint;

        internal Modifier(
            Quaternion? point,
            Vector3? vectorPoint,
            IValues[]? values,
            Modifier<Quaternion>[] modifiers,
            Operation operation)
            : base(point, values, modifiers, operation, ARRAY_SIZE)
        {
            _vectorPoint = vectorPoint;
            _values = values;
        }

        public override Quaternion Point => Modifiers.Length == 0 ? OriginalPoint : Quaternion.Euler(VectorPoint);

        protected override string FormattedValue => Vector3PointDefinition.Format(OriginalPoint.eulerAngles);

        private Vector3 VectorPoint
        {
            get
            {
                Vector3 original = _vectorPoint ??
                                   TranslateEuler(_values ?? throw new InvalidOperationException());
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

        protected override Quaternion Translate(float[] array)
        {
            return Quaternion.Euler(array[0], array[1], array[2]);
        }

        private Vector3 TranslateEuler(IValues[] values)
        {
            int i = 0;
            foreach (IValues value in values)
            {
                foreach (float valueValue in value.Values)
                {
                    _reusableArray[i++] = valueValue;
                    if (i >= ARRAY_SIZE)
                    {
                        break;
                    }
                }
            }

            return new Vector3(_reusableArray[0], _reusableArray[1], _reusableArray[2]);
        }
    }
}
