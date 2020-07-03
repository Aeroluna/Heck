namespace NoodleExtensions.Animation
{
    using System.Linq;
    using CustomJSONData;
    using UnityEngine;

    internal class AnimationController : MonoBehaviour
    {
        private BeatmapObjectSpawnController _beatmapObjectSpawnController;

        internal static AnimationController Instance { get; private set; }

        internal CustomEventCallbackController CustomEventCallbackController { get; private set; }

        internal BeatmapObjectSpawnController BeatmapObjectSpawnController
        {
            get
            {
                if (_beatmapObjectSpawnController == null)
                {
                    _beatmapObjectSpawnController = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First();
                }

                return _beatmapObjectSpawnController;
            }
        }

        internal static void CustomEventCallbackInit(CustomEventCallbackController customEventCallbackController)
        {
            if (Instance != null)
            {
                Destroy(Instance);
            }

            Instance = customEventCallbackController.gameObject.AddComponent<AnimationController>();

            Instance.CustomEventCallbackController = customEventCallbackController;
            Instance.CustomEventCallbackController.AddCustomEventCallback(AnimateTrack.Callback);
            Instance.CustomEventCallbackController.AddCustomEventCallback(AssignPathAnimation.Callback);
            Instance.CustomEventCallbackController.AddCustomEventCallback(AssignPlayerToTrack.Callback);
        }
    }
}
