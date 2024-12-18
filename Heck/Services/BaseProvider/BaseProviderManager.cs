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

internal class BaseProviderManager : ITickable
{
    private readonly Dictionary<string, IValues> _baseProviders = new();
    private readonly HashSet<UpdateableValues> _updateableBaseProviders = [];
    private readonly Dictionary<string, UpdateableValues> _updateableProviders = new();

    [UsedImplicitly]
    private BaseProviderManager(
        [Inject(Optional = true, Source = InjectSources.Local)]
        IEnumerable<IBaseProvider> baseProviders)
    {
        Instance = this;
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

    // I couldnt think of a way to di this thing
    internal static BaseProviderManager Instance { get; private set; } = null!;

    public void Tick()
    {
        foreach (UpdateableValues updatableProvidersValue in _updateableProviders.Values)
        {
            updatableProvidersValue.Update();
        }

        foreach (UpdateableValues updateableBaseProvider in _updateableBaseProviders)
        {
            updateableBaseProvider.Update();
        }
    }

    // Clear cache when song ends to not keep updating unneeded values
    public void Clear()
    {
        _updateableProviders.Clear();
    }

    internal IValues GetProviderValues(string key)
    {
        string[] splits = key.Split('.');
        IValues result = _baseProviders[splits[0]];

        if (splits.Length == 1)
        {
            return result;
        }

        if (_updateableProviders.TryGetValue(key, out UpdateableValues cachedValues))
        {
            return cachedValues;
        }

        for (int i = 1; i < splits.Length; i++)
        {
            string split = splits[i];
            string subKey = string.Join(".", splits.Take(i));
            if (!_updateableProviders.TryGetValue(subKey, out UpdateableValues updateableValues))
            {
                if (split.StartsWith("s"))
                {
                    float mult = float.Parse(split.Substring(1).Replace('-', '.'));
                    updateableValues = result is IRotationValues rotationValues
                        ? new SmoothRotationProvidersValues(rotationValues, mult)
                        : new SmoothProvidersValues(result.Values, mult);
                }
                else
                {
                    int[] parts = split
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

                    updateableValues = new PartialProviderValues(result.Values, parts);
                }

                _updateableProviders[subKey] = updateableValues;
            }

            result = updateableValues;
        }

        return result;
    }
}
