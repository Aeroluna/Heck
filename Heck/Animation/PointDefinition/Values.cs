using System;
using System.Collections.Generic;
using System.Linq;
using Heck.BaseProvider;
using UnityEngine;

namespace Heck.Animation;

internal interface IValues
{
    public float[] Values { get; }
}

internal interface IRotationValues
{
    public Quaternion Rotation { get; }
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

internal abstract record UpdateableValues : IValues
{
    public abstract float[] Values { get; }

    public abstract void Update();
}

internal record QuaternionProviderValues : UpdateableValues, IRotationValues
{
    private readonly float[] _source;

    internal QuaternionProviderValues(float[] source)
    {
        _source = source;
        Values = new float[3];
    }

    public override float[] Values { get; }

    public Quaternion Rotation { get; private set; }

    public override void Update()
    {
        Quaternion quaternion = new(_source[0], _source[1], _source[2], _source[3]);
        Rotation = quaternion;
        Vector3 euler = quaternion.eulerAngles;
        Values[0] = euler.x;
        Values[1] = euler.y;
        Values[2] = euler.z;
    }
}

internal record PartialProviderValues : UpdateableValues
{
    private readonly float[] _source;
    private readonly int[] _parts;

    internal PartialProviderValues(float[] source, int[] parts)
    {
        _source = source;
        _parts = parts;
        Values = new float[_parts.Length];
    }

    public override float[] Values { get; }

    public override void Update()
    {
        for (int i = 0; i < _parts.Length; i++)
        {
            Values[i] = _source[_parts[i]];
        }
    }
}

internal record SmoothRotationProvidersValues : UpdateableValues
{
    private readonly IRotationValues _rotationValues;
    private readonly float _mult;

    private Quaternion _lastQuaternion;

    internal SmoothRotationProvidersValues(IRotationValues rotationValues, float mult)
    {
        _rotationValues = rotationValues;
        _mult = mult;
    }

    public override float[] Values { get; } = new float[3];

    public override void Update()
    {
        _lastQuaternion = Quaternion.Slerp(_lastQuaternion, _rotationValues.Rotation, Time.deltaTime * _mult);
        Vector3 euler = _lastQuaternion.eulerAngles;
        Values[0] = euler.x;
        Values[1] = euler.y;
        Values[2] = euler.z;
    }
}

internal record SmoothProvidersValues : UpdateableValues
{
    private readonly float[] _source;
    private readonly float _mult;

    internal SmoothProvidersValues(float[] source, float mult)
    {
        _source = source;
        _mult = mult;
        Values = new float[source.Length];
    }

    public override float[] Values { get; }

    public override void Update()
    {
        float delta = Time.deltaTime * _mult;
        for (int i = 0; i < _source.Length; i++)
        {
            Values[i] = Mathf.Lerp(Values[i], _source[i], delta);
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
            if (end <= open)
            {
                return;
            }

            result.Add(
                new StaticValues(rawValues.Skip(open).Take(end - open).Select(Convert.ToSingle).ToArray()));
        }
    }
}
