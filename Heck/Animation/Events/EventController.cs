using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static Heck.HeckController;

namespace Heck.Animation.Events
{
    internal class EventController : MonoBehaviour
    {
        private CustomEventCallbackController _customEventCallbackController = null!;
        private LazyInject<CoroutineEventManager> _coroutineEventManager = null!;
        private CustomData _customData = null!;

        [UsedImplicitly]
        [Inject]
        internal void Construct(
            CustomEventCallbackController customEventCallbackController,
            LazyInject<CoroutineEventManager> coroutineEventManager,
            IReadonlyBeatmapData beatmapData,
            [Inject(Id = ID)] CustomData customData,
            [Inject(Id = "isMultiplayer")] bool isMultiplayer)
        {
            if (isMultiplayer)
            {
                enabled = false;
                return;
            }

            _customEventCallbackController = customEventCallbackController;
            _coroutineEventManager = coroutineEventManager;
            _customData = customData;
            customEventCallbackController.AddCustomEventCallback(HandleCallback);
        }

        private void HandleCallback(CustomEventData customEventData)
        {
            switch (customEventData.type)
            {
                case ANIMATE_TRACK:
                    _coroutineEventManager.Value.StartEventCoroutine(customEventData, EventType.AnimateTrack);
                    break;
                case ASSIGN_PATH_ANIMATION:
                    _coroutineEventManager.Value.StartEventCoroutine(customEventData, EventType.AssignPathAnimation);
                    break;
                case INVOKE_EVENT:
                    if (_customData.Resolve(customEventData, out HeckInvokeEventData? heckData))
                    {
                        _customEventCallbackController.InvokeCustomEvent(heckData.CustomEventData);
                    }

                    break;
            }
        }
    }
}
