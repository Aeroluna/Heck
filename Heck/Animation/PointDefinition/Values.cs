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
