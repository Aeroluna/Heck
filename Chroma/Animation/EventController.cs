using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck;
using JetBrains.Annotations;
using Zenject;
using static Chroma.ChromaController;

namespace Chroma.Animation
{
    [UsedImplicitly]
    internal class EventController
    {
        private readonly CustomData _customData;
        private readonly LazyInject<ChromaFogController> _fogController;

        internal EventController(
            CustomEventCallbackController customEventCallbackController,
            [Inject(Id = ID)] CustomData customData,
            LazyInject<ChromaFogController> fogController)
        {
            _customData = customData;
            _fogController = fogController;

            customEventCallbackController.AddCustomEventCallback(HandleCallback);
        }

        private void HandleCallback(CustomEventData customEventData)
        {
            if (customEventData.type != ASSIGN_FOG_TRACK)
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
