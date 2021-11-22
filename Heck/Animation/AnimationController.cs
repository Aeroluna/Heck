namespace Heck.Animation
{
    using CustomJSONData;
    using UnityEngine;

    internal class AnimationController : MonoBehaviour
    {
        internal static AnimationController? Instance { get; private set; }

        internal CustomEventCallbackController CustomEventCallbackController { get; private set; } = null!;

        internal BeatmapObjectSpawnController BeatmapObjectSpawnController => HarmonyPatches.BeatmapObjectSpawnControllerStart.BeatmapObjectSpawnController;

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
            customEventCallbackController.AddCustomEventCallback(AnimateTrack.Callback);
            customEventCallbackController.AddCustomEventCallback(AssignPathAnimation.Callback);
            customEventCallbackController.AddCustomEventCallback(InvokeEvent.Callback);
        }
    }
}
