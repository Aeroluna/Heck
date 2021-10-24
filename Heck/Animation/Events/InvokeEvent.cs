namespace Heck.Animation
{
    using CustomJSONData.CustomBeatmap;
    using static Heck.Animation.AnimationController;
    using static Heck.HeckCustomDataManager;
    using static Heck.Plugin;

    internal static class InvokeEvent
    {
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == INVOKEEVENT)
            {
                HeckInvokeEventData? heckData = TryGetEventData<HeckInvokeEventData>(customEventData);
                if (heckData == null)
                {
                    return;
                }

                Instance!.CustomEventCallbackController!.InvokeCustomEvent(heckData.CustomEventData);
            }
        }
    }
}
