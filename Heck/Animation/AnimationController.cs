namespace Heck.Animation
{
    using CustomJSONData;
    using UnityEngine;

    internal class AnimationController : MonoBehaviour
    {
        internal static AnimationController? Instance { get; private set; }

        internal CustomEventCallbackController? CustomEventCallbackController { get; private set; }

        internal BeatmapObjectSpawnController? BeatmapObjectSpawnController => HarmonyPatches.BeatmapObjectSpawnControllerStart.BeatmapObjectSpawnController;

        internal static void CustomEventCallbackInit(CustomEventCallbackController customEventCallbackController)
        {
            if (customEventCallbackController.BeatmapData?.customData.Get<bool>("isMultiplayer") ?? false)
            {
                return;
            }

            if (Instance != null)
            {
                Destroy(Instance);
            }

            Instance = customEventCallbackController.gameObject.AddComponent<AnimationController>();

            Instance.CustomEventCallbackController = customEventCallbackController;
            Instance.CustomEventCallbackController.AddCustomEventCallback(AnimateTrack.Callback);
            Instance.CustomEventCallbackController.AddCustomEventCallback(AssignPathAnimation.Callback);
            Instance.CustomEventCallbackController.AddCustomEventCallback(InvokeEvent.Callback);
        }
    }
}
