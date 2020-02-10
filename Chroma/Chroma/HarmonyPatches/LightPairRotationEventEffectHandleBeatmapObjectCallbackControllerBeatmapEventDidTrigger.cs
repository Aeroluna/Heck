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
using IPA.Utilities;
using System.Reflection;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(LightPairRotationEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    class LightPairRotationEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger {

        public static BeatmapEventData lastLightPairRotationEventEffectData;

        //Laser rotation
        static void Prefix(ref BeatmapEventData beatmapEventData, ref BeatmapEventType ____eventL, ref BeatmapEventType ____eventR) {
            if (beatmapEventData.type == ____eventL || beatmapEventData.type == ____eventR) {
                lastLightPairRotationEventEffectData = beatmapEventData;
            }
        }

        static void Postfix() {
            lastLightPairRotationEventEffectData = null;
        }

    }

}
