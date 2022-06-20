using System;
using CustomJSONData.CustomBeatmap;
using Heck;
using JetBrains.Annotations;
using Zenject;
using static Chroma.ChromaController;

namespace Chroma.Animation
{
    internal class EventController : IDisposable
    {
        private readonly BeatmapCallbacksController _callbacksController;
        private readonly DeserializedData _deserializedData;
        private readonly LazyInject<FogAnimatorV2> _fogController;
        private readonly AnimateComponentEvent _animateComponentEvent;
        private readonly BeatmapDataCallbackWrapper _callbackWrapper;

        [UsedImplicitly]
        internal EventController(
            BeatmapCallbacksController callbacksController,
            [Inject(Id = ID)] DeserializedData deserializedData,
            LazyInject<FogAnimatorV2> fogController,
            AnimateComponentEvent animateComponentEvent)
        {
            _callbacksController = callbacksController;
            _deserializedData = deserializedData;
            _fogController = fogController;
            _animateComponentEvent = animateComponentEvent;

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
                case ASSIGN_FOG_TRACK:
                    if (_deserializedData.Resolve(customEventData, out ChromaAssignFogEventData? chromaData))
                    {
                        _fogController.Value.AssignTrack(chromaData.Track);
                    }

                    break;

                case ANIMATE_COMPONENT:
                    _animateComponentEvent.StartEventCoroutine(customEventData);
                    break;
            }
        }
    }
}
