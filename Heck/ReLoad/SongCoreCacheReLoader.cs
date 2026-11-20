#if !PRE_V1_44_1
using System;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Zenject;

namespace Heck.ReLoad;

// SongCore implements its own caching that we need to invalidate so we can ReLoad™
public class SongCoreCacheReLoader : IInitializable
{
    private readonly ReLoaderLoader _reLoaderLoader;
    private readonly DiContainer _container;

    private Action? _invalidateSongCoreCache;

    [UsedImplicitly]
    private SongCoreCacheReLoader(ReLoaderLoader reLoaderLoader, DiContainer container)
    {
        _reLoaderLoader = reLoaderLoader;
        _container = container;
    }

    public void Initialize()
    {
        // doesnt vanilla already have caching?
        Type? beatmapLevelCacheType = Type.GetType("SongCore.Hooks.BeatmapLevelCache.BeatmapLevelCache, SongCore");
        if (beatmapLevelCacheType == null)
        {
            return;
        }

        object beatmapLevelCache = _container.TryResolve(beatmapLevelCacheType);
        MethodInfo register = AccessTools.Method(beatmapLevelCacheType, "InvalidateDifficulty");
        _invalidateSongCoreCache = (Action)Delegate.CreateDelegate(typeof(Action), beatmapLevelCache, register);

        _reLoaderLoader.RegisterSongCoreCacheReLoader(this);
    }

    public void InvalidateSongCoreCache()
    {
        _invalidateSongCoreCache?.Invoke();
    }
}
#endif
