using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Heck.Animation;
using JetBrains.Annotations;
using ModestTree;
using Zenject;

namespace Heck.BaseProvider;

public class BaseProviderManager : ITickable
{
    private readonly Dictionary<string, IValues> _baseProviders = new();
    private readonly HashSet<UpdateableValues> _updateableBaseProviders = [];
    private readonly Dictionary<string, UpdateableValues> _updateableProviders = new();
    private readonly Dictionary<string, DynamicProvider> _dynamicProviders = new();
    private readonly Dictionary<string, DynamicSeperator> _dynamicSeperators = new();
    private readonly Dictionary<string, UpdateableValues> _updateableDynamicProviders = new();
    private readonly List<DynamicModifier> _dynamicModifiers = new();

    [UsedImplicitly]
    private BaseProviderManager(
        [Inject(Optional = true, Source = InjectSources.Local)]
        IEnumerable<IBaseProvider> baseProviders)
    {
        Instance = this;
        AddModifierHandler(DefaultModifier);
        foreach (IBaseProvider baseProvider in baseProviders)
        {
            foreach (PropertyInfo propertyInfo in baseProvider.GetType().GetProperties(AccessTools.allDeclared))
            {
                string name = $"base{propertyInfo.Name}";
                bool rotation = propertyInfo.HasAttribute(typeof(QuaternionBaseAttribute));

                float[] array = (float[])propertyInfo.GetValue(baseProvider);

                IValues values;
                if (rotation)
                {
                    QuaternionProviderValues quaternionProviderValues = new(array);
                    values = quaternionProviderValues;
                    _updateableBaseProviders.Add(quaternionProviderValues);
                }
                else
                {
                    values = new BaseProviderValues(array);
                }

                _baseProviders.Add(
                    name,
                    values);
            }
        }
    }

    public delegate UpdateableValues? DynamicModifier(string key, IValues provider, out string rest);

    public delegate IValues? DynamicProvider(string key);

    public delegate string DynamicSeperator(string key, out string rest);

    // I couldnt think of a way to di this thing
    internal static BaseProviderManager Instance { get; private set; } = null!;

    public void Tick()
    {
        foreach (UpdateableValues updatableProvidersValue in _updateableProviders.Values)
        {
            updatableProvidersValue.Update();
        }

        foreach (UpdateableValues updatableDynamicProvidersValue in _updateableDynamicProviders.Values)
        {
            updatableDynamicProvidersValue.Update();
        }

        foreach (UpdateableValues updateableBaseProvider in _updateableBaseProviders)
        {
            updateableBaseProvider.Update();
        }
    }

    [PublicAPI]
    public void AddModifierHandler(DynamicModifier modifier)
    {
        _dynamicModifiers.Add(modifier);
    }

    [PublicAPI]
    public void AddDynamicProvider(string startsWith, DynamicProvider dynamicProvider)
    {
        _dynamicProviders.Add(startsWith, dynamicProvider);
    }

    [PublicAPI]
    public void AddDynamicSeperator(string startsWith, DynamicSeperator dynamicProvider)
    {
        _dynamicSeperators.Add(startsWith, dynamicProvider);
    }

    [PublicAPI]
    public IValues GetProviderValues(string key)
    {
        string rest;
        IValues result = GetProvider(key, out rest);

        if (_updateableProviders.TryGetValue(key, out UpdateableValues cachedValues))
        {
            return cachedValues;
        }


        while (rest != String.Empty)
        {
            UpdateableValues updateableValues = ApplyModifiers(rest, result, out rest);
            string subkey = key.Substring(0, key.Length - rest.Length - 1);
            if (!_updateableProviders.ContainsKey(subkey))
            {
                _updateableProviders.Add(subkey, updateableValues);
            }

            result = _updateableProviders[subkey];
        }

        return result;
    }

    internal bool IsProviderString(string key)
    {
        return key.StartsWith("base")
               || _dynamicProviders.Keys.Any(key.StartsWith);
    }

    // Clear cache when song ends to not keep updating unneeded values
    internal void Clear()
    {
        _updateableProviders.Clear();
        _updateableDynamicProviders.Clear();
    }

    private IValues GetProvider(string key, out string modifiers)
    {
        var splits = key.Split(['.'], 2);
        var start = splits[0];
        modifiers = splits.Length > 1 ? splits[1] : string.Empty;
        if (_baseProviders.TryGetValue(splits[0], out var baseProvider))
        {
            return baseProvider;
        }

        foreach (var providerKey in _dynamicProviders.Keys.Where(key.StartsWith))
        {
            if (_dynamicSeperators.TryGetValue(providerKey, out var seperator))
            {
                start = seperator.Invoke(key, out modifiers);
            }

            if (_updateableDynamicProviders.TryGetValue(start, out var dynamicProvider))
            {
                return dynamicProvider;
            }

            var provider = _dynamicProviders[providerKey].Invoke(start);

            if (provider is null)
            {
                continue;
            }

            if (provider is UpdateableValues updateableProvider)
            {
                _updateableDynamicProviders.Add(start, updateableProvider);
            }

            return provider;
        }

        throw new Exception($"No provider found for: \"{key}\"");
    }

    private UpdateableValues ApplyModifiers(string modifiers, IValues baseValue, out string rest)
    {
        foreach (var modifier in _dynamicModifiers)
        {
            var modifiersValue = modifier(modifiers, baseValue, out rest);
            if (modifiersValue is not null)
            {
                return modifiersValue;
            }
        }

        throw new Exception($"No modifier found for: \"{modifiers}\"");
    }

    private UpdateableValues? DefaultModifier(string modifiers, IValues baseValue, out string rest)
    {
        var splits = modifiers.Split(['.'], 2);
        var modifier = splits[0];
        rest = splits.Length > 1 ? splits[1] : string.Empty;

        switch (modifier[0])
        {
            case 's':
                float mult = float.Parse(modifier.Substring(1).Replace('_', '.'));
                return baseValue is IRotationValues rotationValues
                    ? new SmoothRotationProvidersValues(rotationValues, mult)
                    : new SmoothProvidersValues(baseValue.Values, mult);

            case 'x' or 'y' or 'z' or 'w':
                int[] parts = modifier
                    .Select(
                        n => n switch
                        {
                            'x' => 0,
                            'y' => 1,
                            'z' => 2,
                            'w' => 3,
                            _ => throw new ArgumentOutOfRangeException(nameof(n), n, null)
                        })
                    .ToArray();

                return new PartialProviderValues(baseValue.Values, parts);
        }

        return null;
    }
}
