using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;

namespace Heck.Deserialize
{
    public interface IEarlyDeserializer
    {
        public void DeserializeEarly();
    }

    public interface ICustomEventsDeserializer
    {
        public Dictionary<CustomEventData, ICustomEventCustomData> DeserializeCustomEvents();
    }

    public interface IEventsDeserializer
    {
        public Dictionary<BeatmapEventData, IEventCustomData> DeserializeEvents();
    }

    public interface IObjectsDeserializer
    {
        public Dictionary<BeatmapObjectData, IObjectCustomData> DeserializeObjects();
    }
}
