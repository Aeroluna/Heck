using Heck.Settings;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Heck.ReLoad
{
    public class MenuReLoad : ITickable
    {
        private readonly ReLoaderLoader _reLoaderLoader;
        private readonly LevelSelectionNavigationController _levelSelectionNavigationController;

        [UsedImplicitly]
        private MenuReLoad(
            ReLoaderLoader reLoaderLoader,
            LevelSelectionNavigationController levelSelectionNavigationController)
        {
            _reLoaderLoader = reLoaderLoader;
            _levelSelectionNavigationController = levelSelectionNavigationController;
        }

        public void Tick()
        {
            HeckConfig.ReLoaderSettings config = HeckConfig.Instance.ReLoader;
            if (!Input.GetKeyDown(config.Reload))
            {
                return;
            }

            IDifficultyBeatmap? difficultyBeatmap = _levelSelectionNavigationController.selectedDifficultyBeatmap;
            if (difficultyBeatmap != null)
            {
                _reLoaderLoader.Reload(difficultyBeatmap);
            }
        }
    }
}
