using CustomJSONData.CustomBeatmap;
using static Heck.Animation.AnimationController;
using static Heck.HeckController;
using static Heck.HeckCustomDataManager;

namespace Heck.Animation.Events
{
    internal static class InvokeEvent
    {
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type != INVOKE_EVENT)
            {
                return;
            }

            HeckInvokeEventData? heckData = TryGetEventData<HeckInvokeEventData>(customEventData);
            if (heckData == null)
            {
                return;
            }

            Instance.CustomEventCallbackController.InvokeCustomEvent(heckData.CustomEventData);
        }
    }
}
