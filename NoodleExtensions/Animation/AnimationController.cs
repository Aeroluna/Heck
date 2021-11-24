using CustomJSONData;
using NoodleExtensions.Animation.Events;
using UnityEngine;

namespace NoodleExtensions.Animation
{
    internal class AnimationController : MonoBehaviour
    {
        internal static AnimationController Instance { get; private set; } = null!;

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

            customEventCallbackController.AddCustomEventCallback(AssignPlayerToTrack.Callback);
            customEventCallbackController.AddCustomEventCallback(AssignTrackParent.Callback);
        }
    }
}
