namespace Chroma.HarmonyPatches
{
    using Chroma;
    using static ChromaEventDataManager;

    [ChromaPatch(typeof(TrackLaneRingsRotationEffectSpawner))]
    [ChromaPatch("Start")]
    internal static class TrackLaneRingsRotationEffectSpawnerStart
    {
        private static void Prefix(ref TrackLaneRingsRotationEffect ____trackLaneRingsRotationEffect)
        {
            TrackLaneRingsRotationEffect oldRotationEffect = ____trackLaneRingsRotationEffect;
            ChromaRingsRotationEffect newRotationEffect = oldRotationEffect.gameObject.AddComponent<ChromaRingsRotationEffect>();
            newRotationEffect.CopyValues(oldRotationEffect);

            ____trackLaneRingsRotationEffect = newRotationEffect;
        }
    }

    [ChromaPatch(typeof(TrackLaneRingsRotationEffectSpawner))]
    [ChromaPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class TrackLaneRingsRotationEffectSpawnerHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        private static bool Prefix(
            TrackLaneRingsRotationEffectSpawner __instance,
            BeatmapEventData beatmapEventData,
            BeatmapEventType ____beatmapEventType,
            TrackLaneRingsRotationEffect ____trackLaneRingsRotationEffect,
            float ____rotation,
            float ____rotationStep,
            int ____rotationPropagationSpeed,
            float ____rotationFlexySpeed,
            TrackLaneRingsRotationEffectSpawner.RotationStepType ____rotationStepType)
        {
            if (beatmapEventData.type == ____beatmapEventType)
            {
                ChromaRingRotationEventData chromaData = (ChromaRingRotationEventData)ChromaEventDatas[beatmapEventData];

                // Added in 1.8
                float rotationStep = 0f;
                switch (____rotationStepType)
                {
                    case TrackLaneRingsRotationEffectSpawner.RotationStepType.Range0ToMax:
                        rotationStep = UnityEngine.Random.Range(0f, ____rotationStep);
                        break;

                    case TrackLaneRingsRotationEffectSpawner.RotationStepType.Range:
                        rotationStep = UnityEngine.Random.Range(-____rotationStep, ____rotationStep);
                        break;

                    case TrackLaneRingsRotationEffectSpawner.RotationStepType.MaxOr0:
                        rotationStep = (UnityEngine.Random.value < 0.5f) ? ____rotationStep : 0f;
                        break;
                }

                string nameFilter = chromaData.NameFilter;
                if (nameFilter != null)
                {
                    if (!__instance.name.ToLower().Equals(nameFilter.ToLower()))
                    {
                        return false;
                    }
                }

                int? dir = chromaData.Direction;

                bool rotRight;
                if (dir.HasValue)
                {
                    rotRight = UnityEngine.Random.value < 0.5f;
                }
                else
                {
                    rotRight = dir == 1 ? true : false;
                }

                bool? counterSpin = chromaData.CounterSpin;
                if (counterSpin.HasValue && counterSpin == true)
                {
                    if (!__instance.name.Contains("Big"))
                    {
                        rotRight = !rotRight;
                    }
                }

                bool? reset = chromaData.Reset;
                if (reset.HasValue && reset == true)
                {
                    TriggerRotation(____trackLaneRingsRotationEffect, rotRight, ____rotation, 0, 50, 50);
                    return false;
                }

                float step = chromaData.Step.GetValueOrDefault(rotationStep);
                float prop = chromaData.Prop.GetValueOrDefault(____rotationPropagationSpeed);
                float speed = chromaData.Speed.GetValueOrDefault(____rotationFlexySpeed);
                float rotation = chromaData.Rotation.GetValueOrDefault(____rotation);

                float stepMult = chromaData.StepMult;
                float propMult = chromaData.PropMult;
                float speedMult = chromaData.SpeedMult;

                TriggerRotation(____trackLaneRingsRotationEffect, rotRight, rotation, step * stepMult, prop * propMult, speed * speedMult);
                return false;
            }

            return true;
        }

        private static void TriggerRotation(
            TrackLaneRingsRotationEffect trackLaneRingsRotationEffect,
            bool rotRight,
            float rotation,
            float rotationStep,
            float rotationPropagationSpeed,
            float rotationFlexySpeed)
        {
            ((ChromaRingsRotationEffect)trackLaneRingsRotationEffect).AddRingRotationEffect(trackLaneRingsRotationEffect.GetFirstRingDestinationRotationAngle() + (rotation * (rotRight ? -1 : 1)), rotationStep, rotationPropagationSpeed, rotationFlexySpeed);
        }
    }
}
