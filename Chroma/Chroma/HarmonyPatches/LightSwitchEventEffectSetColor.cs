using Chroma.Beatmap.ChromaEvents;
using Chroma.Beatmap.Events;
using Chroma.Beatmap.Z_Testing.ChromaEvents;
using Chroma.Extensions;
using Chroma.Settings;
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
    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch("SetColor")]
    class LightSwitchEventEffectSetColor {

        static bool Prefix(LightSwitchEventEffect __instance, ref Color color) {

            if (LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.overrideLightWithIdActivation != null) {

                LightWithId[] lights = LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.overrideLightWithIdActivation;
                for (int i = 0; i < lights.Length; i++) {
                    lights[i].ColorWasSet(color);
                }

                return false;
            }

            return true;
        }

    }

}
