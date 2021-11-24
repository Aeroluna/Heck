using CustomJSONData;
using Heck.Animation.Events;
using Heck.HarmonyPatches;
using UnityEngine;

namespace Heck.Animation
{
    internal class AnimationController : MonoBehaviour
    {
        internal static AnimationController Instance { get; private set; } = null!;

        internal static BeatmapObjectSpawnController BeatmapObjectSpawnController => BeatmapObjectSpawnControllerStart.BeatmapObjectSpawnController;

        internal CustomEventCallbackController CustomEventCallbackController { get; private set; } = null!;

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
