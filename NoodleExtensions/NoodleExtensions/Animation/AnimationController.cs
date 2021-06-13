namespace NoodleExtensions.Animation
{
    using CustomJSONData;
    using UnityEngine;

    public class AnimationController : MonoBehaviour
    {
        public static AnimationController Instance { get; private set; }

        public CustomEventCallbackController CustomEventCallbackController { get; private set; }

        internal static void CustomEventCallbackInit(CustomEventCallbackController customEventCallbackController)
        {
            if (customEventCallbackController.BeatmapData.customData.Get<bool>("isMultiplayer"))
            {
                return;
            }

            if (Instance != null)
            {
                Destroy(Instance);
            }

            Instance = customEventCallbackController.gameObject.AddComponent<AnimationController>();

            Instance.CustomEventCallbackController = customEventCallbackController;
            Instance.CustomEventCallbackController.AddCustomEventCallback(AssignPlayerToTrack.Callback);
            Instance.CustomEventCallbackController.AddCustomEventCallback(AssignTrackParent.Callback);
        }
    }
}
