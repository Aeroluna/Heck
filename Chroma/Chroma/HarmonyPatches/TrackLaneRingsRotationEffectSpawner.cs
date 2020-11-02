namespace Chroma.HarmonyPatches
{
    using Chroma;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using static Plugin;

    [ChromaPatch(typeof(TrackLaneRingsRotationEffectSpawner))]
    [ChromaPatch("Start")]
    internal static class TrackLaneRingsRotationEffectSpawnerStart
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Prefix(ref TrackLaneRingsRotationEffect ____trackLaneRingsRotationEffect)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
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
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
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
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            if (beatmapEventData.type == ____beatmapEventType)
            {
                if (beatmapEventData is CustomBeatmapEventData customData)
                {
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

                    dynamic dynData = customData.customData;

                    string nameFilter = Trees.at(dynData, NAMEFILTER);
                    if (nameFilter != null)
                    {
                        if (!__instance.name.ToLower().Equals(nameFilter.ToLower()))
                        {
                            return false;
                        }
                    }

                    int? dir = (int?)Trees.at(dynData, DIRECTION);
                    if (!dir.HasValue)
                    {
                        dir = -1;
                    }

                    bool rotRight;
                    if (dir == -1)
                    {
                        rotRight = UnityEngine.Random.value < 0.5f;
                    }
                    else
                    {
                        rotRight = dir == 1 ? true : false;
                    }

                    bool? counterSpin = Trees.at(dynData, COUNTERSPIN);
                    if (counterSpin.HasValue && counterSpin == true)
                    {
                        if (!__instance.name.Contains("Big"))
                        {
                            rotRight = !rotRight;
                        }
                    }

                    bool? reset = Trees.at(dynData, RESET);
                    if (reset.HasValue && reset == true)
                    {
                        TriggerRotation(____trackLaneRingsRotationEffect, rotRight, ____rotation, 0, 50, 50);
                        return false;
                    }

                    float step = ((float?)Trees.at(dynData, STEP)).GetValueOrDefault(rotationStep);
                    float prop = ((float?)Trees.at(dynData, PROP)).GetValueOrDefault(____rotationPropagationSpeed);
                    float speed = ((float?)Trees.at(dynData, SPEED)).GetValueOrDefault(____rotationFlexySpeed);

                    float stepMult = ((float?)Trees.at(dynData, STEPMULT)).GetValueOrDefault(1f);
                    float propMult = ((float?)Trees.at(dynData, PROPMULT)).GetValueOrDefault(1f);
                    float speedMult = ((float?)Trees.at(dynData, SPEEDMULT)).GetValueOrDefault(1f);

                    TriggerRotation(____trackLaneRingsRotationEffect, rotRight, ____rotation, step * stepMult, prop * propMult, speed * speedMult);
                    return false;
                }
            }

            return true;
        }

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void TriggerRotation(
            TrackLaneRingsRotationEffect trackLaneRingsRotationEffect,
            bool rotRight,
            float rotation,
            float rotationStep,
            float rotationPropagationSpeed,
            float rotationFlexySpeed)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            ((ChromaRingsRotationEffect)trackLaneRingsRotationEffect).AddRingRotationEffect(trackLaneRingsRotationEffect.GetFirstRingDestinationRotationAngle() + (rotation * (rotRight ? -1 : 1)), rotationStep, rotationPropagationSpeed, rotationFlexySpeed);
        }
    }
}
