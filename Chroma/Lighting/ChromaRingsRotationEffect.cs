using System.Collections.Generic;
using IPA.Utilities;
using JetBrains.Annotations;
using Zenject;

namespace Chroma.Lighting
{
    // Whole class rewritten to make propagation a float again
    [UsedImplicitly]
    public class ChromaRingsRotationEffect : IFixedTickable
    {
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, TrackLaneRingsManager>.Accessor _trackLaneRingsManagerAccessor = FieldAccessor<TrackLaneRingsRotationEffect, TrackLaneRingsManager>.GetAccessor("_trackLaneRingsManager");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, float>.Accessor _startupRotationAngleAccessor = FieldAccessor<TrackLaneRingsRotationEffect, float>.GetAccessor("_startupRotationAngle");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, float>.Accessor _startupRotationStepAccessor = FieldAccessor<TrackLaneRingsRotationEffect, float>.GetAccessor("_startupRotationStep");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, int>.Accessor _startupRotationPropagationSpeedAccessor = FieldAccessor<TrackLaneRingsRotationEffect, int>.GetAccessor("_startupRotationPropagationSpeed");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, float>.Accessor _startupRotationFlexySpeedAccessor = FieldAccessor<TrackLaneRingsRotationEffect, float>.GetAccessor("_startupRotationFlexySpeed");

        private readonly List<ChromaRotationEffect> _activeChromaRotationEffects = new(20);
        private readonly List<ChromaRotationEffect> _chromaRotationEffectsPool = new(20);

        private readonly TrackLaneRingsManager _trackLaneRingsManager;

        private ChromaRingsRotationEffect(TrackLaneRingsRotationEffect trackLaneRingsRotationEffect)
        {
            _trackLaneRingsManager = _trackLaneRingsManagerAccessor(ref trackLaneRingsRotationEffect);

            for (int i = 0; i < _chromaRotationEffectsPool.Capacity; i++)
            {
                _chromaRotationEffectsPool.Add(new ChromaRotationEffect());
            }

            float startupRotationAngle = _startupRotationAngleAccessor(ref trackLaneRingsRotationEffect);
            float startupRotationStep = _startupRotationStepAccessor(ref trackLaneRingsRotationEffect);
            int startupRotationPropagationSpeed = _startupRotationPropagationSpeedAccessor(ref trackLaneRingsRotationEffect);
            float startupRotationFlexySpeed = _startupRotationFlexySpeedAccessor(ref trackLaneRingsRotationEffect);
            AddRingRotationEffect(startupRotationAngle, startupRotationStep, startupRotationPropagationSpeed, startupRotationFlexySpeed);
        }

        public void AddRingRotationEffect(float angle, float step, float propagationSpeed, float flexySpeed)
        {
            ChromaRotationEffect ringRotationEffect = SpawnChromaRotationEffect();
            ringRotationEffect.ProgressPos = 0;
            ringRotationEffect.RotationAngle = angle;
            ringRotationEffect.RotationStep = step;
            ringRotationEffect.RotationPropagationSpeed = propagationSpeed;
            ringRotationEffect.RotationFlexySpeed = flexySpeed;
            _activeChromaRotationEffects.Add(ringRotationEffect);
        }

        public void FixedTick()
        {
            TrackLaneRing[] rings = _trackLaneRingsManager.Rings;
            for (int i = _activeChromaRotationEffects.Count - 1; i >= 0; i--)
            {
                ChromaRotationEffect ringRotationEffect = _activeChromaRotationEffects[i];
                long num = (long)ringRotationEffect.ProgressPos;
                ringRotationEffect.ProgressPos += ringRotationEffect.RotationPropagationSpeed;

                while (num < ringRotationEffect.ProgressPos && num < rings.Length)
                {
                    rings[num].SetDestRotation(ringRotationEffect.RotationAngle + (num * ringRotationEffect.RotationStep), ringRotationEffect.RotationFlexySpeed);
                    num++;
                }

                if (!(ringRotationEffect.ProgressPos >= rings.Length))
                {
                    continue;
                }

                RecycleChromaRotationEffect(_activeChromaRotationEffects[i]);
                _activeChromaRotationEffects.RemoveAt(i);
            }
        }

        private ChromaRotationEffect SpawnChromaRotationEffect()
        {
            ChromaRotationEffect result;
            if (_chromaRotationEffectsPool.Count > 0)
            {
                result = _chromaRotationEffectsPool[0];
                _chromaRotationEffectsPool.RemoveAt(0);
            }
            else
            {
                result = new ChromaRotationEffect();
            }

            return result;
        }

        private void RecycleChromaRotationEffect(ChromaRotationEffect ringRotationEffect)
        {
            _chromaRotationEffectsPool.Add(ringRotationEffect);
        }

        [UsedImplicitly]
        internal class Factory : PlaceholderFactory<TrackLaneRingsRotationEffect, ChromaRingsRotationEffect>
        {
        }

        [UsedImplicitly]
        internal class ChromaRingFactory : IFactory<TrackLaneRingsRotationEffect, ChromaRingsRotationEffect>
        {
            private readonly IInstantiator _container;
            private readonly TickableManager _tickableManager;

            private ChromaRingFactory(IInstantiator container, TickableManager tickableManager)
            {
                _container = container;
                _tickableManager = tickableManager;
            }

            public ChromaRingsRotationEffect Create(TrackLaneRingsRotationEffect trackLaneRingsRotationEffect)
            {
                ChromaRingsRotationEffect chromaRing = _container.Instantiate<ChromaRingsRotationEffect>(new[] { trackLaneRingsRotationEffect });
                _tickableManager.AddFixed(chromaRing);
                return chromaRing;
            }
        }

        private class ChromaRotationEffect
        {
            internal float RotationAngle { get; set; }

            internal float RotationStep { get; set; }

            internal float RotationFlexySpeed { get; set; }

            internal float RotationPropagationSpeed { get; set; }

            internal float ProgressPos { get; set; }
        }
    }
}
