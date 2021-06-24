namespace Chroma
{
    using System.Collections.Generic;
    using IPA.Utilities;

    // Whole class rewritten to make propagation a float again
    internal class ChromaRingsRotationEffect : TrackLaneRingsRotationEffect
    {
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, TrackLaneRingsManager>.Accessor _trackLaneRingsManagerAccessor = FieldAccessor<TrackLaneRingsRotationEffect, TrackLaneRingsManager>.GetAccessor("_trackLaneRingsManager");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, float>.Accessor _startupRotationAngleAccessor = FieldAccessor<TrackLaneRingsRotationEffect, float>.GetAccessor("_startupRotationAngle");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, float>.Accessor _startupRotationStepAccessor = FieldAccessor<TrackLaneRingsRotationEffect, float>.GetAccessor("_startupRotationStep");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, int>.Accessor _startupRotationPropagationSpeedAccessor = FieldAccessor<TrackLaneRingsRotationEffect, int>.GetAccessor("_startupRotationPropagationSpeed");
        private static readonly FieldAccessor<TrackLaneRingsRotationEffect, float>.Accessor _startupRotationFlexySpeedAccessor = FieldAccessor<TrackLaneRingsRotationEffect, float>.GetAccessor("_startupRotationFlexySpeed");

        private new List<ChromaRotationEffect> _activeRingRotationEffects;
        private new List<ChromaRotationEffect> _ringRotationEffectsPool;

        public override void AddRingRotationEffect(float angle, float step, int propagationSpeed, float flexySpeed)
        {
            AddRingRotationEffect(angle, step, propagationSpeed, flexySpeed);
        }

        public void AddRingRotationEffect(float angle, float step, float propagationSpeed, float flexySpeed)
        {
            ChromaRotationEffect ringRotationEffect = SpawnRingRotationEffect();
            ringRotationEffect.ProgressPos = 0;
            ringRotationEffect.RotationAngle = angle;
            ringRotationEffect.RotationStep = step;
            ringRotationEffect.RotationPropagationSpeed = propagationSpeed;
            ringRotationEffect.RotationFlexySpeed = flexySpeed;
            _activeRingRotationEffects.Add(ringRotationEffect);
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

        private new void Awake()
        {
            _activeRingRotationEffects = new List<ChromaRotationEffect>(20);
            _ringRotationEffectsPool = new List<ChromaRotationEffect>(20);
            for (int i = 0; i < _ringRotationEffectsPool.Capacity; i++)
            {
                _ringRotationEffectsPool.Add(new ChromaRotationEffect());
            }
        }

        private new void Start()
        {
            AddRingRotationEffect(_startupRotationAngle, _startupRotationStep, _startupRotationPropagationSpeed, _startupRotationFlexySpeed);
        }

        private new void FixedUpdate()
        {
            TrackLaneRing[] rings = _trackLaneRingsManager.Rings;
            for (int i = _activeRingRotationEffects.Count - 1; i >= 0; i--)
            {
                ChromaRotationEffect ringRotationEffect = _activeRingRotationEffects[i];
                long num = (long)ringRotationEffect.ProgressPos;
                ringRotationEffect.ProgressPos += ringRotationEffect.RotationPropagationSpeed;
                while (num < ringRotationEffect.ProgressPos && num < rings.Length)
                {
                    rings[num].SetDestRotation(ringRotationEffect.RotationAngle + (num * ringRotationEffect.RotationStep), ringRotationEffect.RotationFlexySpeed);
                    num++;
                }

                if (ringRotationEffect.ProgressPos >= rings.Length)
                {
                    RecycleRingRotationEffect(_activeRingRotationEffects[i]);
                    _activeRingRotationEffects.RemoveAt(i);
                }
            }
        }

        private new ChromaRotationEffect SpawnRingRotationEffect()
        {
            ChromaRotationEffect result;
            if (_ringRotationEffectsPool.Count > 0)
            {
                result = _ringRotationEffectsPool[0];
                _ringRotationEffectsPool.RemoveAt(0);
            }
            else
            {
                result = new ChromaRotationEffect();
            }

            return result;
        }

        private void RecycleRingRotationEffect(ChromaRotationEffect ringRotationEffect)
        {
            _ringRotationEffectsPool.Add(ringRotationEffect);
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
