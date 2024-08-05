using System;
using JetBrains.Annotations;

namespace Heck.SettingsSetter;

public interface ISettableSetting
{
    string FieldName { get; }

    string GroupName { get; }

    object TrueValue { get; }

    void SetTemporary(object? tempValue);
}

public class SettableSetting<T> : ISettableSetting
    where T : struct
{
    private T? _tempValue;
    private T _value;

    public SettableSetting(string groupName, string fieldName)
    {
        GroupName = groupName;
        FieldName = fieldName;
    }

    [PublicAPI]
    public event Action? ValueChanged;

    public string FieldName { get; }

    public string GroupName { get; }

    public object TrueValue => _value;

    public T Value
    {
        get => _tempValue ?? _value;
        set
        {
            _value = value;
            ValueChanged?.Invoke();
        }
    }

    public void SetTemporary(object? tempValue)
    {
        _tempValue = (T?)tempValue;
        ValueChanged?.Invoke();
    }
}
