using System;
using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using Zenject;

namespace Heck
{
    public sealed class DisposableClassFactory<TParam, TType> : IFactory<TParam, TType>, IDisposable
        where TType : IDisposable
    {
        private readonly IInstantiator _instantiator;
        private readonly HashSet<TType> _activeObjects = new();

        [UsedImplicitly]
        private DisposableClassFactory(IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }

        public void Dispose()
        {
            _activeObjects.Do(n => n.Dispose());
        }

        public TType Create(TParam param)
        {
            TType createdObject = _instantiator.Instantiate<TType>(new object[] { param! });
            _activeObjects.Add(createdObject);
            return createdObject;
        }
    }
}
