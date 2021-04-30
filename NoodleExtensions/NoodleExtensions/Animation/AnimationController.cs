namespace NoodleExtensions.Animation
{
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;

    public class AnimationController : MonoBehaviour
    {
        public static AnimationController Instance { get; private set; }

        public CustomEventCallbackController CustomEventCallbackController { get; private set; }

        internal static void CustomEventCallbackInit(CustomEventCallbackController customEventCallbackController)
        {
            if (customEventCallbackController._beatmapData is CustomBeatmapData customBeatmapData && Trees.at(customBeatmapData.customData, "isMultiplayer") != null)
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
