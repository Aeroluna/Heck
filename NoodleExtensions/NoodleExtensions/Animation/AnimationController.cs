namespace NoodleExtensions.Animation
{
    using System.Linq;
    using CustomJSONData;
    using UnityEngine;

    public class AnimationController : MonoBehaviour
    {
        private BeatmapObjectSpawnController _beatmapObjectSpawnController;

        public static AnimationController Instance { get; private set; }

        public CustomEventCallbackController CustomEventCallbackController { get; private set; }

        public BeatmapObjectSpawnController BeatmapObjectSpawnController
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
