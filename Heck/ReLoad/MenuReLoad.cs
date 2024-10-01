using Heck.Settings;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Heck.ReLoad;

public class MenuReLoad : ITickable
{
    private readonly Config.ReLoaderSettings _config;
#if PRE_V1_37_1
    private readonly LevelSelectionNavigationController _levelSelectionNavigationController;
#endif
    private readonly ReLoaderLoader _reLoaderLoader;

    [UsedImplicitly]
    private MenuReLoad(
        ReLoaderLoader reLoaderLoader,
#if PRE_V1_37_1
        LevelSelectionNavigationController levelSelectionNavigationController,
#endif
        Config.ReLoaderSettings config)
    {
        _reLoaderLoader = reLoaderLoader;
#if PRE_V1_37_1
        _levelSelectionNavigationController = levelSelectionNavigationController;
#endif
        _config = config;
    }

    public void Tick()
    {
        if (!Input.GetKeyDown(_config.Reload))
        {
            return;
        }

#if !PRE_V1_37_1
        _reLoaderLoader.Reload();
#else
        IDifficultyBeatmap? difficultyBeatmap = _levelSelectionNavigationController.selectedDifficultyBeatmap;
        if (difficultyBeatmap != null)
        {
            _reLoaderLoader.Reload(difficultyBeatmap);
        }
#endif
    }
}
