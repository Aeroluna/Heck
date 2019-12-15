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

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(LightPairRotationEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    class LightPairRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger {

        /*public static BeatmapEventData lastLightPairRotationEventEffectData;

        //Laser rotation
        static void Prefix(ref BeatmapEventData beatmapEventData) {
            lastLightPairRotationEventEffectData = beatmapEventData;
        }

        static void Postfix() {
            lastLightPairRotationEventEffectData = null;
        }*/

        /*static bool Prefix(ref LightPairRotationEventEffect __instance, ref BeatmapEventData beatmapEventData, ref BeatmapEventType ____event, ref Transform ____transform, ref Quaternion ____startRotation, ref Vector3 ____rotationVector) {
            if (beatmapEventData.type == this._eventL || beatmapEventData.type == this._eventR) {
                int frameCount = Time.frameCount;
                if (this._randomGenerationFrameNum != frameCount) {
                    if (this._overrideRandomValues) {
                        this._randomDirection = ((beatmapEventData.type == this._eventL) ? 1f : -1f);
                        this._randomStartRotation = (float)((beatmapEventData.type == this._eventL) ? frameCount : (-(float)frameCount));
                    } else {
                        this._randomDirection = ((UnityEngine.Random.value > 0.5f) ? 1f : -1f);
                        this._randomStartRotation = UnityEngine.Random.Range(0f, 360f);
                    }
                    this._randomGenerationFrameNum = Time.frameCount;
                }
                if (beatmapEventData.type == this._eventL) {
                    this.UpdateRotationData(beatmapEventData.value, this._rotationDataL, this._randomStartRotation, this._randomDirection);
                } else if (beatmapEventData.type == this._eventR) {
                    this.UpdateRotationData(beatmapEventData.value, this._rotationDataR, -this._randomStartRotation, -this._randomDirection);
                }
                base.enabled = (this._rotationDataL.enabled || this._rotationDataR.enabled);
            }
        }*/

    }

}
