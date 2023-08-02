using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Chroma.EnvironmentEnhancement.Saved
{
    internal class ReloadListener : ITickable
    {
        private readonly SavedEnvironmentLoader _savedEnvironmentLoader;
        private readonly BeatmapDataCache _beatmapDataCache;

        [UsedImplicitly]
        private ReloadListener(SavedEnvironmentLoader savedEnvironmentLoader, BeatmapDataCache beatmapDataCache)
        {
            _savedEnvironmentLoader = savedEnvironmentLoader;
            _beatmapDataCache = beatmapDataCache;
        }

        public void Tick()
        {
            if (!Input.GetKey(KeyCode.LeftControl) || !Input.GetKeyDown(KeyCode.E))
            {
                return;
            }

            _beatmapDataCache.difficultyBeatmap = null;
            _savedEnvironmentLoader.Init();
        }
    }
}
