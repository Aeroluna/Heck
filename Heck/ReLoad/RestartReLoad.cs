using Heck.Settings;
using JetBrains.Annotations;
using SiraUtil.Affinity;

namespace Heck.ReLoad;

internal class ReLoadRestart : IAffinity
{
    private readonly Config.ReLoaderSettings _config;
    private readonly ReLoaderLoader _reLoaderLoader;

    [UsedImplicitly]
    private ReLoadRestart(
        ReLoaderLoader reLoaderLoader,
        Config.ReLoaderSettings config)
    {
        _reLoaderLoader = reLoaderLoader;
        _config = config;
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(MenuTransitionsHelper), nameof(MenuTransitionsHelper.HandleMainGameSceneDidFinish))]
    private void Prefix(
        LevelCompletionResults levelCompletionResults,
        StandardLevelScenesTransitionSetupDataSO ____standardLevelScenesTransitionSetupData)
    {
        if (levelCompletionResults.levelEndAction != LevelCompletionResults.LevelEndAction.Restart)
        {
            return;
        }

        if (_config.ReloadOnRestart &&
            ____standardLevelScenesTransitionSetupData.practiceSettings != null)
        {
#if !PRE_V1_37_1
            _reLoaderLoader.Reload();
#else
            _reLoaderLoader.Reload(____standardLevelScenesTransitionSetupData.difficultyBeatmap);
#endif
        }
    }
}
