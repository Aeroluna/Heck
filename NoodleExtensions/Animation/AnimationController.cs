namespace NoodleExtensions.Animation
{
    using CustomJSONData;
    using UnityEngine;

    internal class AnimationController : MonoBehaviour
    {
        internal static AnimationController? Instance { get; private set; }

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
            customEventCallbackController.AddCustomEventCallback(AssignPlayerToTrack.Callback);
            customEventCallbackController.AddCustomEventCallback(AssignTrackParent.Callback);
        }
    }
}
