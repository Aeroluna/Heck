using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Zenject;

namespace Heck.BaseProvider
{
    internal class BaseProviderManager
    {
        private readonly Dictionary<string, BaseProviderData> _baseProviders = new();

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
                    BaseProviderAttribute? attribute = propertyInfo.GetCustomAttribute<BaseProviderAttribute>();
                    if (attribute == null)
                    {
                        continue;
                    }

                    _baseProviders.Add(attribute.Name, new BaseProviderData(baseProvider, propertyInfo.GetMethod));
                }
            }
        }

        // I couldnt think of a way to di this thing
        internal static BaseProviderManager Instance { get; private set; } = null!;

        internal BaseProviderData GetProviderData(string key)
        {
            return _baseProviders[key];
        }
    }

    internal class BaseProviderData
    {
        private readonly IBaseProvider _baseProvider;
        private readonly MethodInfo _getter;

        internal BaseProviderData(IBaseProvider baseProvider, MethodInfo getter)
        {
            _baseProvider = baseProvider;
            _getter = getter;
        }

        // TODO: use delegates instead for better performance
        internal object GetValue()
        {
            object result = _getter.Invoke(_baseProvider, Array.Empty<object>());
            return result;
        }
    }
}
