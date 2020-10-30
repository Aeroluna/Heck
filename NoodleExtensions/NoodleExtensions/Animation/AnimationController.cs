namespace NoodleExtensions.Animation
{
    using CustomJSONData;
    using UnityEngine;

    public class AnimationController : MonoBehaviour
    {
        public static AnimationController Instance { get; private set; }

        public CustomEventCallbackController CustomEventCallbackController { get; private set; }

        public BeatmapObjectSpawnController BeatmapObjectSpawnController => HarmonyPatches.BeatmapObjectSpawnControllerStart.BeatmapObjectSpawnController;

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
            Instance.CustomEventCallbackController.AddCustomEventCallback(AssignTrackParent.Callback);
        }
    }
}
