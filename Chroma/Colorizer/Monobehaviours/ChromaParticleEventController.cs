namespace Chroma.Colorizer
{
    using UnityEngine;

    internal class ChromaParticleEventController : MonoBehaviour
    {
        private ParticleColorizer _colorizer = null!;
        private BeatmapEventType _eventType;

        internal void Init(ParticleSystemEventEffect particleSystemEventEffect, BeatmapEventType eventType)
        {
            _eventType = eventType;
            _colorizer = new ParticleColorizer(particleSystemEventEffect, eventType);
        }

        private void OnDestroy()
        {
            _colorizer.UnsubscribeEvent();
            _eventType.GetParticleColorizers().Remove(_colorizer);
        }
    }
}
