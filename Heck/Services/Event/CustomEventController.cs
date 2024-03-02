using System;
using System.Collections.Generic;
using System.Reflection;
using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using SiraUtil.Logging;

namespace Heck.Event
{
    internal class CustomEventController : IDisposable
    {
        private readonly Dictionary<string, ICustomEvent> _customEvents;

        private readonly BeatmapCallbacksController _callbacksController;
        private readonly BeatmapDataCallbackWrapper _callbackWrapper;

        [UsedImplicitly]
        private CustomEventController(
            SiraLog log,
            BeatmapCallbacksController callbacksController,
            List<ICustomEvent> customEvents)
        {
            _callbacksController = callbacksController;
            _callbackWrapper = callbacksController.AddBeatmapCallback<CustomEventData>(HandleCallback);
            _customEvents = new Dictionary<string, ICustomEvent>(customEvents.Count);
            foreach (ICustomEvent customEvent in customEvents)
            {
                Type systemType = customEvent.GetType();
                CustomEvent? attribute = systemType.GetCustomAttribute<CustomEvent>();
                if (attribute == null)
                {
                    log.Warn($"[{systemType.FullName}] is missing CustomEvent attribute and will be ignored.");
                    continue;
                }

                foreach (string type in attribute.Type)
                {
                    _customEvents.Add(type, customEvent);
                }
            }
        }

        public void Dispose()
        {
            _callbacksController.RemoveBeatmapCallback(_callbackWrapper);
        }

        private void HandleCallback(CustomEventData customEventData)
        {
            if (_customEvents.TryGetValue(customEventData.eventType, out ICustomEvent customEvent))
            {
                customEvent.Callback(customEventData);
            }
        }
    }
}
