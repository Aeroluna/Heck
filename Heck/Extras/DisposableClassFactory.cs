using System;
using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using Zenject;

namespace Heck;

public sealed class DisposableClassFactory<TParam, TType> : IFactory<TParam, TType>, IDisposable
    where TType : IDisposable
{
    private readonly HashSet<TType> _activeObjects = [];
    private readonly IInstantiator _instantiator;

    [UsedImplicitly]
    private DisposableClassFactory(IInstantiator instantiator)
    {
        _instantiator = instantiator;
    }

    public TType Create(TParam param)
    {
        TType createdObject = _instantiator.Instantiate<TType>([param]);
        _activeObjects.Add(createdObject);
        return createdObject;
    }

    public void Dispose()
    {
        _activeObjects.Do(n => n.Dispose());
    }
}
