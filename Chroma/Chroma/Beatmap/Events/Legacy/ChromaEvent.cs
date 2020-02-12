using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Beatmap.Events.Legacy
{
    public abstract class ChromaEvent
    {
        private static Dictionary<BeatmapEventData, ChromaEvent> chromaEvents = new Dictionary<BeatmapEventData, ChromaEvent>();

        public static void ClearChromaEvents()
        {
            chromaEvents.Clear();
        }

        public static ChromaEvent SetChromaEvent(BeatmapEventData lightEvent, ChromaEvent chromaEvent)
        {
            if (chromaEvents.TryGetValue(lightEvent, out ChromaEvent value))
            {
                chromaEvents.Remove(lightEvent);
            }
            chromaEvents.Add(lightEvent, chromaEvent);
            chromaEvent.OnEventSet(lightEvent);
            return chromaEvent;
        }

        public static void RemoveChromaEvent(BeatmapEventData beatmapEvent)
        {
            if (chromaEvents.ContainsKey(beatmapEvent)) chromaEvents.Remove(beatmapEvent);
        }

        public static ChromaEvent GetChromaEvent(BeatmapEventData beatmapEvent)
        {
            if (chromaEvents.ContainsKey(beatmapEvent))
            {
                if (chromaEvents.TryGetValue(beatmapEvent, out ChromaEvent chromaEvent)) return chromaEvent;
            }
            return null;
        }

        protected BeatmapEventData data;

        public BeatmapEventData Data
        {
            get
            {
                return data;
            }
        }

        protected bool requiresColourEventsEnabled;

        public bool RequiresColourEventsEnabled { get { return requiresColourEventsEnabled; } }

        public ChromaEvent(BeatmapEventData data, bool requiresColourEventsEnabled)
        {
            this.data = data;
            this.requiresColourEventsEnabled = requiresColourEventsEnabled;
        }

        public abstract bool Activate(ref MonoBehaviour light, ref BeatmapEventData data, ref BeatmapEventType eventType);

        public virtual void OnEventSet(BeatmapEventData lightmapEvent)
        {
        }
    }
}