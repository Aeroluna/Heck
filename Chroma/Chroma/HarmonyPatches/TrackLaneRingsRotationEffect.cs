namespace Chroma.HarmonyPatches
{
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;

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

                    string nameFilter = Trees.at(dynData, "_nameFilter");
                    if (nameFilter != null)
                    {
                        if (!__instance.name.ToLower().Equals(nameFilter.ToLower()))
                        {
                            return false;
                        }
                    }

                    int? dir = (int?)Trees.at(dynData, "_direction");
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

                    bool? counterSpin = Trees.at(dynData, "_counterSpin");
                    if (counterSpin.HasValue && counterSpin == true)
                    {
                        if (!__instance.name.Contains("Big"))
                        {
                            rotRight = !rotRight;
                        }
                    }

                    bool? reset = Trees.at(dynData, "_reset");
                    if (reset.HasValue && reset == true)
                    {
                        TriggerRotation(____trackLaneRingsRotationEffect, rotRight, ____rotation, 0, 50, 50);
                        return false;
                    }

                    float step = ((float?)Trees.at(dynData, "_step")).GetValueOrDefault(rotationStep);
                    int prop = ((int?)Trees.at(dynData, "_prop")).GetValueOrDefault(____rotationPropagationSpeed);
                    float speed = ((float?)Trees.at(dynData, "_speed")).GetValueOrDefault(____rotationFlexySpeed);

                    float stepMult = ((float?)Trees.at(dynData, "_stepMult")).GetValueOrDefault(1f);
                    float propMult = ((float?)Trees.at(dynData, "_propMult")).GetValueOrDefault(1f);
                    float speedMult = ((float?)Trees.at(dynData, "_speedMult")).GetValueOrDefault(1f);

                    TriggerRotation(____trackLaneRingsRotationEffect, rotRight, ____rotation, step * stepMult, (int)(prop * propMult), speed * speedMult);
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
            int rotationPropagationSpeed,
            float rotationFlexySpeed)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            trackLaneRingsRotationEffect.AddRingRotationEffect(trackLaneRingsRotationEffect.GetFirstRingDestinationRotationAngle() + (rotation * (rotRight ? -1 : 1)), rotationStep, rotationPropagationSpeed, rotationFlexySpeed);
        }
    }
}
