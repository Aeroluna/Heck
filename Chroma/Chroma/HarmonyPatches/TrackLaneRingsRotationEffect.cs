using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(TrackLaneRingsRotationEffectSpawner))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal class TrackLaneRingsRotationEffectSpawnerHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        //Ring rotation
        private static bool Prefix(TrackLaneRingsRotationEffectSpawner __instance, BeatmapEventData beatmapEventData, BeatmapEventType ____beatmapEventType,
            TrackLaneRingsRotationEffect ____trackLaneRingsRotationEffect, float ____rotationStep, int ____rotationPropagationSpeed, float ____rotationFlexySpeed,
            TrackLaneRingsRotationEffectSpawner.RotationStepType ____rotationStepType)
        {
            try
            {
                if (beatmapEventData.type == ____beatmapEventType && ChromaBehaviour.LightingRegistered)
                {
                    if (beatmapEventData is CustomBeatmapEventData customData)
                    {
                        // Added in 1.8
                        float step = 0f;
                        switch (____rotationStepType)
                        {
                            case TrackLaneRingsRotationEffectSpawner.RotationStepType.Range0ToMax:
                                step = UnityEngine.Random.Range(0f, ____rotationStep);
                                break;

                            case TrackLaneRingsRotationEffectSpawner.RotationStepType.Range:
                                step = UnityEngine.Random.Range(-____rotationStep, ____rotationStep);
                                break;

                            case TrackLaneRingsRotationEffectSpawner.RotationStepType.MaxOr0:
                                step = ((UnityEngine.Random.value < 0.5f) ? ____rotationStep : 0f);
                                break;
                        }

                        dynamic dynData = customData.customData;

                        string nameFilter = Trees.at(dynData, "_nameFilter");
                        if (nameFilter != null)
                        {
                            if (!__instance.name.ToLower().Contains(nameFilter.ToLower())) return false;
                        }

                        bool? reset = Trees.at(dynData, "_reset");
                        if (reset.HasValue && reset == true)
                        {
                            ResetRings(__instance, ____trackLaneRingsRotationEffect, ____rotationStep, ____rotationPropagationSpeed, ____rotationFlexySpeed);
                            return false;
                        }

                        float? stepMult = (float?)Trees.at(dynData, "_stepMult");
                        stepMult = stepMult.GetValueOrDefault(1f);
                        float? propMult = (float?)Trees.at(dynData, "_propMult");
                        propMult = propMult.GetValueOrDefault(1f);
                        float? speedMult = (float?)Trees.at(dynData, "_speedMult");
                        speedMult = speedMult.GetValueOrDefault(1f);

                        int? dir = (int?)Trees.at(dynData, "_direction");
                        if (!dir.HasValue) dir = -1;

                        bool rotRight;
                        if (dir == -1)
                        {
                            rotRight = UnityEngine.Random.value < 0.5f;
                        }
                        else rotRight = dir == 1 ? true : false;

                        bool? counterSpin = Trees.at(dynData, "_counterSpin");
                        if (counterSpin.HasValue && counterSpin == true)
                        {
                            if (__instance.name.ToLower().Contains("small")) rotRight = !rotRight;
                        }

                        TriggerRotation(rotRight, __instance, ____trackLaneRingsRotationEffect, step, ____rotationPropagationSpeed, ____rotationFlexySpeed, stepMult.Value,
                            propMult.Value, speedMult.Value);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log("INVALID _customData", Logger.Level.WARNING);
                Logger.Log(e);
            }

            return true;
        }

        private static void TriggerRotation(bool rotRight, TrackLaneRingsRotationEffectSpawner __instance, TrackLaneRingsRotationEffect ____trackLaneRingsRotationEffect,
            float ____rotationStep, int ____rotationPropagationSpeed, float ____rotationFlexySpeed, float ringStepMult = 1f, float ringPropagationMult = 1f, float ringSpeedMult = 1f)
        {
            ____trackLaneRingsRotationEffect.AddRingRotationEffect(____trackLaneRingsRotationEffect.GetFirstRingDestinationRotationAngle() + (90 * (rotRight ? -1 : 1)), ____rotationStep * ringStepMult, (int)(____rotationPropagationSpeed * ringPropagationMult), ____rotationFlexySpeed * ringSpeedMult);
        }

        private static void ResetRings(TrackLaneRingsRotationEffectSpawner __instance, TrackLaneRingsRotationEffect ____trackLaneRingsRotationEffect, float ____rotationStep,
            int ____rotationPropagationSpeed, float ____rotationFlexySpeed)
        {
            TriggerRotation(UnityEngine.Random.value < 0.5f, __instance, ____trackLaneRingsRotationEffect, ____rotationStep, ____rotationPropagationSpeed, ____rotationFlexySpeed, 0f, 116f, 116f);
        }
    }
}