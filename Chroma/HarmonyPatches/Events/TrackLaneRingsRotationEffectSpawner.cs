using Chroma.Lighting;
using Heck;
using JetBrains.Annotations;
using UnityEngine;
using static Chroma.ChromaCustomDataManager;

namespace Chroma.HarmonyPatches.Events
{
    [HeckPatch(typeof(TrackLaneRingsRotationEffectSpawner))]
    [HeckPatch("Start")]
    internal static class TrackLaneRingsRotationEffectSpawnerStart
    {
        [UsedImplicitly]
        private static void Prefix(ref TrackLaneRingsRotationEffect ____trackLaneRingsRotationEffect)
        {
            if (____trackLaneRingsRotationEffect.GetType() != typeof(TrackLaneRingsRotationEffect))
            {
                return;
            }

            TrackLaneRingsRotationEffect oldRotationEffect = ____trackLaneRingsRotationEffect;
            ChromaRingsRotationEffect newRotationEffect = oldRotationEffect.gameObject.AddComponent<ChromaRingsRotationEffect>();
            newRotationEffect.CopyValues(oldRotationEffect);
            Object.Destroy(oldRotationEffect);

            ____trackLaneRingsRotationEffect = newRotationEffect;
        }
    }

    [HeckPatch(typeof(TrackLaneRingsRotationEffectSpawner))]
    [HeckPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class TrackLaneRingsRotationEffectSpawnerHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        [UsedImplicitly]
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
            if (beatmapEventData.type != ____beatmapEventType)
            {
                return true;
            }

            ChromaEventData? chromaData = TryGetEventData(beatmapEventData);
            if (chromaData == null)
            {
                return true;
            }

            // Added in 1.8
            float rotationStep = ____rotationStepType switch
            {
                TrackLaneRingsRotationEffectSpawner.RotationStepType.Range0ToMax =>
                    Random.Range(0f, ____rotationStep),
                TrackLaneRingsRotationEffectSpawner.RotationStepType.Range =>
                    Random.Range(-____rotationStep, ____rotationStep),
                TrackLaneRingsRotationEffectSpawner.RotationStepType.MaxOr0 =>
                    (Random.value < 0.5f) ? ____rotationStep : 0f,
                _ => 0f
            };

            string? nameFilter = chromaData.NameFilter;
            if (nameFilter != null)
            {
                if (!__instance.name.ToLower().Equals(nameFilter.ToLower()))
                {
                    return false;
                }
            }

            int? dir = chromaData.Direction;

            bool rotRight;
            if (!dir.HasValue)
            {
                rotRight = Random.value < 0.5f;
            }
            else
            {
                rotRight = dir == 1;
            }

            bool? counterSpin = chromaData.CounterSpin;
            if (counterSpin is true)
            {
                if (!__instance.name.Contains("Big"))
                {
                    rotRight = !rotRight;
                }
            }

            bool? reset = chromaData.Reset;
            if (reset is true)
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

        private static void TriggerRotation(
            TrackLaneRingsRotationEffect trackLaneRingsRotationEffect,
            bool rotRight,
            float rotation,
            float rotationStep,
            float rotationPropagationSpeed,
            float rotationFlexySpeed)
        {
            ((ChromaRingsRotationEffect)trackLaneRingsRotationEffect).AddRingRotationEffect(
                trackLaneRingsRotationEffect.GetFirstRingDestinationRotationAngle() + (rotation * (rotRight ? -1 : 1)),
                rotationStep,
                rotationPropagationSpeed,
                rotationFlexySpeed);
        }
    }
}
