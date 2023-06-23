using System;
using BepInEx.Configuration;
using JetBrains.Annotations;

namespace Heck.SettingsSetter
{
    public interface ISettableSetting
    {
        string GroupName { get; }

        string FieldName { get; }

        object? TrueValue { get; }

        void SetTemporary(object? tempValue);
    }

    public class SettableSetting<T> : ISettableSetting
        where T : struct
    {
        private T _value;

        private T? _tempValue;

        public SettableSetting(string groupName, string fieldName)
        {
            GroupName = groupName;
            FieldName = fieldName;
        }

        [PublicAPI]
        public event Action? ValueChanged;

        public string GroupName { get; }

        public string FieldName { get; }

        public object? TrueValue => _value;

        public T Value
        {
            get => _tempValue ?? _value;
            set
            {
                _value = value;
                ValueChanged?.Invoke();
            }
        }

        public static implicit operator T(SettableSetting<T> settableSetting) => settableSetting.Value;

        public void SetTemporary(object? tempValue)
        {
            _tempValue = (T?)tempValue;
            ValueChanged?.Invoke();
        }
    }

    public class SettableConfigEntry<T> : ISettableSetting
    {
        private T? _tempValue;

        public SettableConfigEntry(ConfigEntry<T> configEntry, string groupName, string fieldName)
        {
            ConfigEntry = configEntry;
            GroupName = groupName;
            FieldName = fieldName;
        }

        public string GroupName { get; }

        public string FieldName { get; }

        public object? TrueValue => ConfigEntry.Value;

        public ConfigEntry<T> ConfigEntry { get; }

        public T Value
        {
            get => _tempValue ?? ConfigEntry.Value;
            set => ConfigEntry.Value = value;
        }

        public static implicit operator T(SettableConfigEntry<T> settableConfigEntry) => settableConfigEntry.Value;

        public void SetTemporary(object? tempValue)
        {
            _tempValue = (T?)tempValue;
        }
    }
}
