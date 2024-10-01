using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Chroma.EnvironmentEnhancement.Saved;

internal class ReloadListener : ITickable
{
    private readonly SavedEnvironmentLoader _savedEnvironmentLoader;

#if !PRE_V1_37_1
    private readonly BeatmapDataLoader _beatmapDataLoader;
#else
    private readonly BeatmapDataCache _beatmapDataCache;
#endif

    [UsedImplicitly]
    private ReloadListener(
        SavedEnvironmentLoader savedEnvironmentLoader,
#if !PRE_V1_37_1
        BeatmapDataLoader beatmapDataLoader)
#else
        BeatmapDataCache beatmapDataCache)
#endif
    {
        _savedEnvironmentLoader = savedEnvironmentLoader;
#if !PRE_V1_37_1
        _beatmapDataLoader = beatmapDataLoader;
#else
        _beatmapDataCache = beatmapDataCache;
#endif
    }

    public void Tick()
    {
        if (!Input.GetKey(KeyCode.LeftControl) || !Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

#if !PRE_V1_37_1
        _beatmapDataLoader._lastUsedBeatmapDataCache = default;
#else
        _beatmapDataCache.difficultyBeatmap = null;
#endif
        _savedEnvironmentLoader.Init();
    }
}
