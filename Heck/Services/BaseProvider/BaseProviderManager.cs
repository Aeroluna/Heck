using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Heck.Animation;
using JetBrains.Annotations;
using Zenject;

namespace Heck.BaseProvider;

internal class BaseProviderManager
{
    private readonly Dictionary<string, BaseProviderValues> _baseProviders = new();

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

    internal BaseProviderValues GetProviderValues(string key)
    {
        return _baseProviders[key];
    }
}

internal struct BaseProviderValues : IValues
{
    internal BaseProviderValues(float[] values)
    {
        Values = values;
    }

    public float[] Values { get; }
}
