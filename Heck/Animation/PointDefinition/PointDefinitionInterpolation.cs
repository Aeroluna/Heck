using System;
using UnityEngine;

namespace Heck.Animation;

internal interface IPointDefinitionInterpolation
{
    public float Time { set; }

    public void Finish();

    public void Init(IPointDefinition? pointDefinition);
}

internal abstract class PointDefinitionInterpolation<T> : IPointDefinitionInterpolation
    where T : struct
{
    private PointDefinition<T>? _basePointData;
    private PointDefinition<T>? _previousPointData;

    public float Time { get; set; }

    public static PointDefinitionInterpolation<T> CreateDerived()
    {
        PointDefinitionInterpolation<T>? result = typeof(T) switch
        {
            var n when n == typeof(float) => new FloatPointDefinitionInterpolation() as PointDefinitionInterpolation<T>,
            var n when n == typeof(Vector3) =>
                new Vector3PointDefinitionInterpolation() as PointDefinitionInterpolation<T>,
            var n when n == typeof(Vector4) =>
                new Vector4PointDefinitionInterpolation() as PointDefinitionInterpolation<T>,
            var n when n == typeof(Quaternion) =>
                new QuaternionPointDefinitionInterpolation() as PointDefinitionInterpolation<T>,
            _ => throw new ArgumentOutOfRangeException(nameof(T))
        };

        return result ?? throw new InvalidCastException("Invalid cast.");
    }

    public void Finish()
    {
        _previousPointData = null;
    }

    public void Init(IPointDefinition? newPointData)
    {
        Time = 0;
        _previousPointData = _basePointData;
        if (newPointData == null)
        {
            _basePointData = null;
            return;
        }

        if (newPointData is not PointDefinition<T> casted)
        {
            throw new InvalidOperationException();
        }

        _basePointData = casted;
    }

    public override string ToString()
    {
        return $"({_previousPointData?.ToString() ?? "null"}, {_basePointData?.ToString() ?? "null"})";
    }

    internal T? Interpolate(float time)
    {
        if (_basePointData == null)
        {
            return null;
        }

        return _previousPointData == null
            ? _basePointData.Interpolate(time)
            : InterpolatePoints(_previousPointData, _basePointData, Time, time);
    }

    protected abstract T InterpolatePoints(
        PointDefinition<T> previousPoint,
        PointDefinition<T> basePoint,
        float interpolation,
        float time);
}
