using CustomJSONData;
using UnityEngine;

namespace NoodleExtensions.Animation
{
    internal class AnimationController : MonoBehaviour
    {
        internal static AnimationController _instance;

        internal static CustomEventCallbackController _customEventCallbackController;

        internal static void CustomEventCallbackInit(CustomEventCallbackController customEventCallbackController)
        {
            _customEventCallbackController = customEventCallbackController;
            _customEventCallbackController.AddCustomEventCallback(Dissolve.Callback);
            _customEventCallbackController.AddCustomEventCallback(DissolveArrow.Callback);
            _customEventCallbackController.AddCustomEventCallback(AnimateTrack.Callback);
            _customEventCallbackController.AddCustomEventCallback(AssignPathAnimation.Callback);

            if (_instance != null) Destroy(_instance);
            _instance = _customEventCallbackController.gameObject.AddComponent<AnimationController>();
        }
    }
}