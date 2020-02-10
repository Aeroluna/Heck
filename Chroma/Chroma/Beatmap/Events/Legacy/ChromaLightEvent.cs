using UnityEngine;

namespace Chroma.Beatmap.Events.Legacy
{
    public class ChromaLightEvent : ChromaColourEvent
    {
        public const int CHROMA_LIGHT_OFFSET = 1900000000;

        public ChromaLightEvent(BeatmapEventData data, Color a, Color b) : base(data, true, a, b)
        {
        }

        public override bool Activate(ref MonoBehaviour light, ref BeatmapEventData data, ref BeatmapEventType eventType)
        {
            ColourManager.RecolourLight(ref light, A == null ? (Color)ColourManager.LightA : A, B == null ? (Color)ColourManager.LightB : B);
            return true;
        }
    }
}