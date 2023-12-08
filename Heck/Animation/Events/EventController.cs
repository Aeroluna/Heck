using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using static AlphabetScrollInfo;
using static Heck.HeckController;

namespace Heck.Animation.Events
{
    internal class EventController : IDisposable
    {
        private readonly BeatmapCallbacksController _callbacksController;
        private readonly LazyInject<CoroutineEventManager> _coroutineEventManager;
        private readonly BeatmapDataCallbackWrapper _callbackWrapper;
        private readonly BeatmapObjectManager _beatmapObjectManager;

        [UsedImplicitly]
        private EventController(
            BeatmapCallbacksController callbacksController,
            LazyInject<CoroutineEventManager> coroutineEventManager,
            BeatmapObjectManager beatmapObjectManager)
        {
            _callbacksController = callbacksController;
            _coroutineEventManager = coroutineEventManager;
            _callbackWrapper = callbacksController.AddBeatmapCallback<CustomEventData>(HandleCallback);
            _beatmapObjectManager = beatmapObjectManager;
            _beatmapObjectManager.noteWasCutEvent += BeatmapObjectManager_noteWasCutEvent;
            _beatmapObjectManager.noteWasMissedEvent += BeatmapObjectManager_noteWasMissedEvent;
        }

        private void BeatmapObjectManager_noteWasMissedEvent(NoteController noteController)
        {
            CustomNoteData noteData = (CustomNoteData)noteController._noteData;
            IEnumerable<Trigger> triggers = Trigger.GetTriggers(noteData.customData, "triggerOnMiss");
            if (triggers == null)
            {
                return;
            }

            foreach (Trigger trigger in triggers)
            {
                trigger.isTriggered = true;
            }
        }

        private void BeatmapObjectManager_noteWasCutEvent(NoteController noteController, in NoteCutInfo _)
        {
            CustomNoteData noteData = (CustomNoteData)noteController._noteData;
            IEnumerable<Trigger> triggers = Trigger.GetTriggers(noteData.customData, "triggerOnCut");
            if (triggers == null)
            {
                return;
            }

            foreach (Trigger trigger in triggers)
            {
                trigger.isTriggered = true;
            }
        }

        public void Dispose()
        {
            _callbacksController.RemoveBeatmapCallback(_callbackWrapper);
            _beatmapObjectManager.noteWasCutEvent -= BeatmapObjectManager_noteWasCutEvent;
            _beatmapObjectManager.noteWasMissedEvent -= BeatmapObjectManager_noteWasMissedEvent;
            Trigger.triggers.Clear();
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
