using System;
using JetBrains.Annotations;

namespace Heck.BaseProvider;

internal class GameBaseProviderDisposer : IDisposable
{
    private readonly BaseProviderManager _baseProviderManager;

    [UsedImplicitly]
    private GameBaseProviderDisposer(BaseProviderManager baseProviderManager)
    {
        _baseProviderManager = baseProviderManager;
    }

    public void Dispose()
    {
        _baseProviderManager.Clear();
    }
}
