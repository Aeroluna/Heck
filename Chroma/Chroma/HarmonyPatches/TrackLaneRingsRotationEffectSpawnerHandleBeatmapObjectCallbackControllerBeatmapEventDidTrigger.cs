using Chroma.Beatmap.Events;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(TrackLaneRingsRotationEffectSpawner))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    class TrackLaneRingsRotationEffectSpawnerHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger {

        //Ring rotation
        static bool Prefix(TrackLaneRingsRotationEffectSpawner __instance, ref BeatmapEventData beatmapEventData, ref BeatmapEventType ____beatmapEventType, ref TrackLaneRingsRotationEffect ____trackLaneRingsRotationEffect, ref float ____rotationStep, ref float ____rotationPropagationSpeed, ref float ____rotationFlexySpeed) {

            if (ChromaEvent.SimpleEventActivate(__instance, ref beatmapEventData, ref ____beatmapEventType)) return false;

            //ChromaLogger.Log("Ring rotation type " + ____beatmapEventType + " v :" + beatmapEventData.value);

            if (beatmapEventData.value == ChromaEvent.CHROMA_EVENT_RING_ROTATE_LEFT || beatmapEventData.value == ChromaEvent.CHROMA_EVENT_RING_ROTATE_RIGHT) {
                //ChromaLogger.Log("Ring event " + beatmapEventData.value);
                ____trackLaneRingsRotationEffect.AddRingRotationEffect(____trackLaneRingsRotationEffect.GetFirstRingDestinationRotationAngle() + (float)(90 * ((beatmapEventData.value == ChromaEvent.CHROMA_EVENT_RING_ROTATE_RIGHT) ? -1 : 1)), UnityEngine.Random.Range(0f, ____rotationStep * ChromaRingStepEvent.ringStepMult), ____rotationPropagationSpeed * ChromaRingPropagationEvent.ringPropagationMult, ____rotationFlexySpeed * ChromaRingSpeedEvent.ringSpeedMult);
                return false;
            }

            if (beatmapEventData.type == ____beatmapEventType && (ChromaRingSpeedEvent.ringSpeedMult != 1f || ChromaRingPropagationEvent.ringPropagationMult != 1f || ChromaRingStepEvent.ringStepMult != 1f)) {
                ____trackLaneRingsRotationEffect.AddRingRotationEffect(____trackLaneRingsRotationEffect.GetFirstRingDestinationRotationAngle() + (float)(90 * ((UnityEngine.Random.value >= 0.5f) ? -1 : 1)), UnityEngine.Random.Range(0f, ____rotationStep * ChromaRingStepEvent.ringStepMult), ____rotationPropagationSpeed * ChromaRingPropagationEvent.ringPropagationMult, ____rotationFlexySpeed * ChromaRingSpeedEvent.ringSpeedMult);
                return false;
            }

            return true;
        }

    }

}
