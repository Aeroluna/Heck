using System;
using System.Collections.Generic;
using System.Reflection;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;

namespace Heck.Deserialize
{
    internal class DataDeserializer
    {
        private readonly ConstructorInfo _constructor;

        private object? _instance;

        internal DataDeserializer(string? id, Type type)
        {
            _constructor = AccessTools.FirstConstructor(type, _ => true);
            Id = id;
        }

        internal bool Enabled { get; set; }

        internal string? Id { get; }

        public override string ToString()
        {
            return Id ?? "NULL";
        }

        internal void Create(object[] inputs)
        {
            _instance = _constructor.Invoke(_constructor.ActualParameters(inputs));
            if (_instance is IEarlyDeserializer earlyDeserializer)
            {
                earlyDeserializer.DeserializeEarly();
            }
        }

        internal DeserializedData Deserialize()
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("No instance found");
            }

            Dictionary<CustomEventData, ICustomEventCustomData>? customEventDatas = null;
            if (_instance is ICustomEventsDeserializer customEventsDeserializer)
            {
                customEventDatas = customEventsDeserializer.DeserializeCustomEvents();
            }

            Dictionary<BeatmapEventData, IEventCustomData>? eventDatas = null;
            if (_instance is IEventsDeserializer eventsDeserializer)
            {
                eventDatas = eventsDeserializer.DeserializeEvents();
            }

            Dictionary<BeatmapObjectData, IObjectCustomData>? objectDatas = null;
            if (_instance is IObjectsDeserializer objectsDeserializer)
            {
                objectDatas = objectsDeserializer.DeserializeObjects();
            }

            customEventDatas ??= new Dictionary<CustomEventData, ICustomEventCustomData>();
            eventDatas ??= new Dictionary<BeatmapEventData, IEventCustomData>();
            objectDatas ??= new Dictionary<BeatmapObjectData, IObjectCustomData>();

            return new DeserializedData(customEventDatas, eventDatas, objectDatas);
        }
    }
}
