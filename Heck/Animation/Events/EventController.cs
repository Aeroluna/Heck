using System;
using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using Zenject;
using static Heck.HeckController;

namespace Heck.Animation.Events
{
    internal class EventController : IDisposable
    {
        private readonly BeatmapCallbacksController _callbacksController;
        private readonly LazyInject<CoroutineEventManager> _coroutineEventManager;
        private readonly BeatmapDataCallbackWrapper _callbackWrapper;

        [UsedImplicitly]
        private EventController(
            BeatmapCallbacksController callbacksController,
            LazyInject<CoroutineEventManager> coroutineEventManager)
        {
            _callbacksController = callbacksController;
            _coroutineEventManager = coroutineEventManager;
            _callbackWrapper = callbacksController.AddBeatmapCallback<CustomEventData>(HandleCallback);
        }

        public void Dispose()
        {
            _callbacksController.RemoveBeatmapCallback(_callbackWrapper);
        }

        private void HandleCallback(CustomEventData customEventData)
        {
            switch (customEventData.eventType)
            {
                case ANIMATE_TRACK:
                    _coroutineEventManager.Value.StartEventCoroutine(customEventData, EventType.AnimateTrack);
                    break;
                case ASSIGN_PATH_ANIMATION:
                    _coroutineEventManager.Value.StartEventCoroutine(customEventData, EventType.AssignPathAnimation);
                    break;

                // TODO: reimplement this
                /*case INVOKE_EVENT:
                    if (_customData.Resolve(customEventData, out HeckInvokeEventData? heckData))
                    {
                        _customEventCallbackController.InvokeCustomEvent(heckData.CustomEventData);
                    }

                    break;*/
            }
        }
    }
}
