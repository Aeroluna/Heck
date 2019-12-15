using Chroma.Beatmap.Events;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.HarmonyPatches {

    /*[HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(LightPairRotationEventEffect))]
    [HarmonyPatch("UpdateRotationData")]
    class LightPairRotationEventEffectUpdateRotationData {

        //Laser rotation
        static bool Prefix(LightPairRotationEventEffect __instance, ref LightPairRotationEventEffect.RotationData rotationData, ref float ____startRotation, ref Vector3 ____rotationVector) {

            BeatmapEventData beatmapEventData = LightPairRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.lastLightPairRotationEventEffectData;

            if (beatmapEventData.type == ____event) {

                ChromaLogger.Log("rot event " + ____event.ToString());

                if (beatmapEventData is CustomBeatmapEventData customData) {
                    dynamic dynData = customData.customData;
                    if (dynData != null) {
                        
                        bool? lockPosition = Trees.at(dynData, "_lockPosition");
                        if (lockPosition == null) lockPosition = false;

                        double? precisionSpeed = Trees.at(dynData, "_preciseSpeed");
                        if (precisionSpeed == null) precisionSpeed = (float)beatmapEventData.value;

                        int? dir = Trees.at(dynData, "_direction");
                        if (dir == null) dir = 0;

                        bool rotInboard;
                        if (dir == 0) rotInboard = UnityEngine.Random.value < 0.5f;
                        else if (dir == 1) rotInboard = true;
                        else rotInboard = false;

                        //Actual lasering
                        if (precisionSpeed == 0) {
                            __instance.enabled = false;
                            if (!(bool)lockPosition) ____transform.localRotation = ____startRotation;
                        } else if (!(bool)lockPosition) {
                            ____transform.localRotation = ____startRotation;
                            ____transform.Rotate(____rotationVector, UnityEngine.Random.Range(0f, 180f), Space.Self);
                        }
                        __instance.enabled = true;
                        ____rotationSpeed = (float)precisionSpeed * 20f * (rotInboard ? -1f : 1f);

                        return false;
                    }
                }
            }

            return true;
        }

    }*/

}
