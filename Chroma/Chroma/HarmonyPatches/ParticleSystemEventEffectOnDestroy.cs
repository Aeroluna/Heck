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
    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("OnDestroy")]
    class ParticleSystemEventEffectOnDestroy
    {

        static void Postfix(ParticleSystemEventEffect __instance, ref BeatmapEventType ____colorEvent) {
            LightSwitchEventEffectExtensions.LSEDestroy(__instance, ____colorEvent);
        }

    }

}
