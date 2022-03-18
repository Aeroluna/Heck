using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static Heck.HeckController;

namespace Heck.Animation.Events
{
    internal class EventController : MonoBehaviour
    {
        private LazyInject<CoroutineEventManager> _coroutineEventManager = null!;

        [UsedImplicitly]
        [Inject]
        internal void Construct(
            BeatmapCallbacksController beatmapCallbacksController,
            LazyInject<CoroutineEventManager> coroutineEventManager,
            IReadonlyBeatmapData beatmapData,
            [Inject(Id = "isMultiplayer")] bool isMultiplayer)
        {
            if (isMultiplayer)
            {
                enabled = false;
                return;
            }

            _coroutineEventManager = coroutineEventManager;
            beatmapCallbacksController.AddBeatmapCallback<CustomEventData>(HandleCallback);
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
