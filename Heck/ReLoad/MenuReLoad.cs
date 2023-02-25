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
        private readonly Config.ReLoaderSettings _config;

        [UsedImplicitly]
        private MenuReLoad(
            ReLoaderLoader reLoaderLoader,
            LevelSelectionNavigationController levelSelectionNavigationController,
            Config.ReLoaderSettings config)
        {
            _reLoaderLoader = reLoaderLoader;
            _levelSelectionNavigationController = levelSelectionNavigationController;
            _config = config;
        }

        public void Tick()
        {
            if (!Input.GetKeyDown(_config.Reload))
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
