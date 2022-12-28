using Heck.ReLoad;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Heck
{
    // Dummy monobehaviour because coroutines need a monobehaviour
    public class CoroutineDummy : MonoBehaviour
    {
        private ReLoader? _reloader;

        [Inject]
        [UsedImplicitly]
        private void Construct([InjectOptional] ReLoader? reloader)
        {
            _reloader = reloader;
            if (reloader != null)
            {
                reloader.Rewinded += OnRewind;
            }
        }

        private void OnDestroy()
        {
            if (_reloader != null)
            {
                _reloader.Rewinded -= OnRewind;
            }
        }

        private void OnRewind()
        {
            StopAllCoroutines();
        }
    }
}
