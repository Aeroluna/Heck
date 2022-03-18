using System;
using CustomJSONData.CustomBeatmap;
using Heck;
using JetBrains.Annotations;
using Zenject;
using static Chroma.ChromaController;

namespace Chroma.Animation
{
    [UsedImplicitly]
    internal class EventController : IDisposable
    {
        private readonly BeatmapCallbacksController _callbacksController;
        private readonly CustomData _customData;
        private readonly LazyInject<ChromaFogController> _fogController;
        private readonly BeatmapDataCallbackWrapper _callbackWrapper;

        internal EventController(
            BeatmapCallbacksController callbacksController,
            [Inject(Id = ID)] CustomData customData,
            LazyInject<ChromaFogController> fogController)
        {
            _callbacksController = callbacksController;
            _customData = customData;
            _fogController = fogController;

            _callbackWrapper = callbacksController.AddBeatmapCallback<CustomEventData>(HandleCallback);
        }

        public void Dispose()
        {
            _callbacksController.RemoveBeatmapCallback(_callbackWrapper);
        }

        private void HandleCallback(CustomEventData customEventData)
        {
            if (customEventData.eventType != ASSIGN_FOG_TRACK)
            {
                return;
            }

            if (_customData.Resolve(customEventData, out ChromaCustomEventData? chromaData))
            {
                _fogController.Value.AssignTrack(chromaData.Track);
            }
        }
    }
}
