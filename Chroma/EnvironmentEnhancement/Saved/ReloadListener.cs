using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Chroma.EnvironmentEnhancement.Saved
{
    internal class ReloadListener : ITickable
    {
        private readonly SavedEnvironmentLoader _savedEnvironmentLoader;

        [UsedImplicitly]
        private ReloadListener(SavedEnvironmentLoader savedEnvironmentLoader)
        {
            _savedEnvironmentLoader = savedEnvironmentLoader;
        }

        public void Tick()
        {
            if (!Input.GetKey(KeyCode.LeftControl) || !Input.GetKeyDown(KeyCode.E))
            {
                return;
            }

            _savedEnvironmentLoader.Init();
        }
    }
}
