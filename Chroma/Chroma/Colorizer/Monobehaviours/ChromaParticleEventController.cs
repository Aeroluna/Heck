namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using UnityEngine;

    internal class ChromaParticleEventController : MonoBehaviour
    {
        private ParticleColorizer _colorizer;
        private BeatmapEventType _eventType;

        internal void Init(ParticleSystemEventEffect particleSystemEventEffect, BeatmapEventType eventType)
        {
            _eventType = eventType;
            _colorizer = new ParticleColorizer(particleSystemEventEffect, eventType);
        }

        private void OnDestroy()
        {
            if (_colorizer != null)
            {
                _colorizer.UnsubscribeEvent();
                _eventType.GetParticleColorizers().Remove(_colorizer);
            }
        }
    }
}
