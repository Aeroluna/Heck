using Chroma.Beatmap.Events;
using Chroma.Extensions;
using Chroma.Settings;
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
    [HarmonyPatch("Start")]
    class LightSwitchEventEffectStart {

        static void Postfix(LightSwitchEventEffect __instance) {
            LightSwitchEventEffectExtensions.LSEStart(__instance);
        }

    }

}
