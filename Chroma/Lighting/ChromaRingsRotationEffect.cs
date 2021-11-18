namespace Chroma
{
    using System.Collections.Generic;
    using IPA.Utilities;

    // Whole class rewritten to make propagation a float again
    public class ChromaRingsRotationEffect : TrackLaneRingsRotationEffect
    {
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, TrackLaneRingsManager>.Accessor _trackLaneRingsManagerAccessor = FieldAccessor<TrackLaneRingsRotationEffect, TrackLaneRingsManager>.GetAccessor("_trackLaneRingsManager");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, float>.Accessor _startupRotationAngleAccessor = FieldAccessor<TrackLaneRingsRotationEffect, float>.GetAccessor("_startupRotationAngle");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, float>.Accessor _startupRotationStepAccessor = FieldAccessor<TrackLaneRingsRotationEffect, float>.GetAccessor("_startupRotationStep");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, int>.Accessor _startupRotationPropagationSpeedAccessor = FieldAccessor<TrackLaneRingsRotationEffect, int>.GetAccessor("_startupRotationPropagationSpeed");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, float>.Accessor _startupRotationFlexySpeedAccessor = FieldAccessor<TrackLaneRingsRotationEffect, float>.GetAccessor("_startupRotationFlexySpeed");

        private readonly List<ChromaRotationEffect> _activeChromaRotationEffects = new List<ChromaRotationEffect>(20);
        private readonly List<ChromaRotationEffect> _chromaRotationEffectsPool = new List<ChromaRotationEffect>(20);

        public override void AddRingRotationEffect(float angle, float step, int propagationSpeed, float flexySpeed)
        {
            AddRingRotationEffect(angle, step, propagationSpeed, flexySpeed);
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

        public override void Awake()
        {
            for (int i = 0; i < _chromaRotationEffectsPool.Capacity; i++)
            {
                _chromaRotationEffectsPool.Add(new ChromaRotationEffect());
            }
        }

        public override void Start()
        {
            AddRingRotationEffect(_startupRotationAngle, _startupRotationStep, _startupRotationPropagationSpeed, _startupRotationFlexySpeed);
        }

        public override void FixedUpdate()
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

                if (ringRotationEffect.ProgressPos >= rings.Length)
                {
                    RecycleChromaRotationEffect(_activeChromaRotationEffects[i]);
                    _activeChromaRotationEffects.RemoveAt(i);
                }
            }
        }

        internal void SetNewRingManager(TrackLaneRingsManager trackLaneRingsManager)
        {
            _trackLaneRingsManager = trackLaneRingsManager;
        }

        internal void CopyValues(TrackLaneRingsRotationEffect trackLaneRingsRotationEffect)
        {
            _trackLaneRingsManager = _trackLaneRingsManagerAccessor(ref trackLaneRingsRotationEffect);
            _startupRotationAngle = _startupRotationAngleAccessor(ref trackLaneRingsRotationEffect);
            _startupRotationStep = _startupRotationStepAccessor(ref trackLaneRingsRotationEffect);
            _startupRotationPropagationSpeed = _startupRotationPropagationSpeedAccessor(ref trackLaneRingsRotationEffect);
            _startupRotationFlexySpeed = _startupRotationFlexySpeedAccessor(ref trackLaneRingsRotationEffect);
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
