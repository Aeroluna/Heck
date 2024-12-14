using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Heck.Animation;

// TODO: allow operations to handle different types
// TODO: add more operations
// ReSharper disable InconsistentNaming
#pragma warning disable SA1300 // Element should begin with upper-case letter
public enum Operation
{
    opNone = 0,
    opAdd,
    opSub,
    opMul,
    opDiv
}

#pragma warning restore SA1300 // Element should begin with upper-case letter

internal abstract class Modifier<T>
    where T : struct
{
    private readonly IValues[]? _values;
    private readonly T? _rawPoint;
    private readonly float[] _reusableArray;

    internal Modifier(T? point, IValues[]? values, Modifier<T>[] modifiers, Operation operation, int arraySize)
    {
        _rawPoint = point;
        _values = values;
        Modifiers = modifiers;
        Operation = operation;
        HasBaseProvider = values != null || modifiers.Any(n => n.HasBaseProvider);
        _reusableArray = new float[arraySize];
    }

    public bool HasBaseProvider { get; }

    public Operation Operation { get; }

    public abstract T Point { get; }

    protected abstract string FormattedValue { get; }

    protected Modifier<T>[] Modifiers { get; }

    protected T OriginalPoint =>
        _rawPoint ?? (_values != null ? Convert(_values) : throw new InvalidOperationException());

    public override string ToString()
    {
        const string spacer = ", ";
        StringBuilder stringBuilder = new("[" + FormattedValue);
        if (Operation != Operation.opNone)
        {
            stringBuilder.Append(spacer + Operation);
        }

        if (Modifiers.Length > 0)
        {
            stringBuilder.Append(string.Join(spacer, (IEnumerable<object>)Modifiers));
        }

        stringBuilder.Append(']');
        return stringBuilder.ToString();
    }

    internal void FillValues(IValues[] values)
    {
        float[] array = _reusableArray;
        int i = 0;
        foreach (IValues value in values)
        {
            foreach (float valueValue in value.Values)
            {
                array[i++] = valueValue;
                if (i >= array.Length)
                {
                    return;
                }
            }
        }

        throw new InvalidOperationException("Not enough values to fill the modifier.");
    }

    protected abstract T Translate(float[] values);

    private T Convert(IValues[] values)
    {
        FillValues(values);
        return Translate(_reusableArray);
    }
}
