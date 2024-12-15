using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Heck.Animation;
using JetBrains.Annotations;
using Zenject;

namespace Heck.BaseProvider;

internal class BaseProviderManager
{
    private readonly Dictionary<string, BaseProviderValues> _baseProviders = new();
    private readonly Dictionary<string, PartialProviderValues> _partialProviders = new();

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
                _baseProviders.Add(name, new BaseProviderValues((float[])propertyInfo.GetValue(baseProvider)));
            }
        }
    }

    // I couldnt think of a way to di this thing
    internal static BaseProviderManager Instance { get; private set; } = null!;

    internal IValues GetProviderValues(string key)
    {
        int index = key.LastIndexOf('.');
        if (index == -1)
        {
            return _baseProviders[key];
        }

        // ReSharper disable once InvertIf
        if (!_partialProviders.TryGetValue(key, out PartialProviderValues partialProvider))
        {
            float[] source = _baseProviders[key.Substring(0, index)].Values;
            string partString = key.Substring(index + 1);
            int[] parts = partString.Select(
                n => n switch
                {
                    'x' => 0,
                    'y' => 1,
                    'z' => 2,
                    'w' => 3,
                    _ => throw new ArgumentOutOfRangeException(nameof(n), n, null)
                }).ToArray();

            partialProvider = new PartialProviderValues(source, parts);
            _partialProviders[key] = partialProvider;
        }

        return partialProvider;
    }
}
