namespace Chroma.Events
{
    using System.Collections.Generic;
    using Chroma.Utils;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;

    internal class ChromaLightColourEvent
    {
        internal static Dictionary<BeatmapEventType, List<TimedColor>> LightColours { get; } = new Dictionary<BeatmapEventType, List<TimedColor>>();

        // Creates dictionary loaded with all _lightRGB custom events and indexs them with the event's time and type
        internal static void Activate(List<CustomEventData> eventData)
        {
            foreach (CustomEventData d in eventData)
            {
                dynamic dynData = d.data;
                int id = (int)Trees.at(dynData, "_event");
                Color c = ChromaUtils.GetColorFromData(dynData);

                // Fuck legacy chroma
                if (!LightColours.TryGetValue((BeatmapEventType)id, out List<TimedColor> dictionaryID))
                {
                    dictionaryID = new List<TimedColor>();
                    LightColours.Add((BeatmapEventType)id, dictionaryID);
                }

                dictionaryID.Add(new TimedColor(d.time, c));
            }
        }
    }
}
