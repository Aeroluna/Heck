using Heck.Settings;
using JetBrains.Annotations;
using SiraUtil.Affinity;

namespace Heck.ReLoad
{
    internal class ReLoadRestart : IAffinity
    {
        private readonly ReLoaderLoader _reLoaderLoader;

        [UsedImplicitly]
        private ReLoadRestart(
            ReLoaderLoader reLoaderLoader)
        {
            _reLoaderLoader = reLoaderLoader;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(MenuTransitionsHelper), nameof(MenuTransitionsHelper.HandleMainGameSceneDidFinish))]
        private void Prefix(LevelCompletionResults levelCompletionResults, StandardLevelScenesTransitionSetupDataSO ____standardLevelScenesTransitionSetupData)
        {
            if (levelCompletionResults.levelEndAction != LevelCompletionResults.LevelEndAction.Restart)
            {
                return;
            }

            HeckConfig.ReLoaderSettings config = HeckConfig.Instance.ReLoader;

            if (config.ReloadOnRestart &&
                ____standardLevelScenesTransitionSetupData.practiceSettings != null)
            {
                _reLoaderLoader.Reload(____standardLevelScenesTransitionSetupData.difficultyBeatmap);
            }
        }
    }
}
