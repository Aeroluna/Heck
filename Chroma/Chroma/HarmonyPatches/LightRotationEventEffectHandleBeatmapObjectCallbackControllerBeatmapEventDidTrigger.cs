using Chroma.Beatmap.Events;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(LightRotationEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    class LightRotationEventEffectEffectSpawnerHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger {

        public static bool IsDefaults() {
            return ChromaEvent.disablePositionReset == false && ChromaEvent.laserSpinDirection == 0;
        }

        //Laser rotation
        static bool Prefix(LightRotationEventEffect __instance, ref BeatmapEventData beatmapEventData, ref BeatmapEventType ____event, ref Transform ____transform, ref Quaternion ____startRotation, ref Vector3 ____rotationVector, ref float ____rotationSpeed) {
            
            if (beatmapEventData.type == ____event) {
                if (!IsDefaults()) {
                    if (beatmapEventData.value == 0) {
                        __instance.enabled = false;
                        if (!ChromaEvent.disablePositionReset) ____transform.localRotation = ____startRotation;
                    } else if (beatmapEventData.value > 0) {
                        if (!ChromaEvent.disablePositionReset) {
                            ____transform.localRotation = ____startRotation;
                            ____transform.Rotate(____rotationVector, UnityEngine.Random.Range(0f, 180f), Space.Self);
                        }
                        __instance.enabled = true;
                        ____rotationSpeed = (float)beatmapEventData.value * 20f * (GetRotationDirection());
                    }
                    return false;
                }
            }

            return true;
        }

        private static float GetRotationDirection() {
            if (ChromaEvent.laserSpinDirection == -1) return -1f;
            else if (ChromaEvent.laserSpinDirection == 1) return 1;
            else return (UnityEngine.Random.value <= 0.5f) ? -1f : 1f;
        }

    }

}
