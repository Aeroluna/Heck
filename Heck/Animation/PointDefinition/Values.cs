using System;
using System.Collections.Generic;
using System.Linq;
using Heck.BaseProvider;

namespace Heck.Animation;

internal interface IValues
{
    public float[] Values { get; }
}

internal readonly struct StaticValues : IValues
{
    internal StaticValues(float[] values)
    {
        Values = values;
    }

    public float[] Values { get; }
}

internal struct BaseProviderValues : IValues
{
    internal BaseProviderValues(float[] values)
    {
        Values = values;
    }

    public float[] Values { get; }
}

internal readonly struct PartialProviderValues : IValues
{
    private readonly float[] _values;
    private readonly float[] _source;
    private readonly int[] _parts;

    internal PartialProviderValues(float[] source, int[] parts)
    {
        _source = source;
        _parts = parts;
        _values = new float[_parts.Length];
    }

    public float[] Values
    {
        get
        {
            for (int i = 0; i < _parts.Length; i++)
            {
                _values[i] = _source[_parts[i]];
            }

            return _values;
        }
    }
}

internal static class ValuesExtensions
{
    internal static IValues[] DeserializeValues(this object[] rawValues)
    {
        List<IValues> result = new(1);
        int start = 0;
        for (int i = 0; i < rawValues.Length; i++)
        {
            if (rawValues[i] is not string s)
            {
                continue;
            }

            Close(start, i);
            start = i + 1;
            result.Add(BaseProviderManager.Instance.GetProviderValues(s));
        }

        Close(start, rawValues.Length);

        return result.ToArray();

        void Close(int open, int end)
        {
            result.Add(
                new StaticValues(rawValues.Skip(open).Take(end).Select(Convert.ToSingle).ToArray()));
        }
    }
}
