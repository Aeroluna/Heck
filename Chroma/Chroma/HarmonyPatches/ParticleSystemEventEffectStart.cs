using Chroma.Beatmap.Events;
using Chroma.Extensions;
using Chroma.Settings;
using Harmony;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("Start")]
    class ParticleSystemEventEffectStart
    {

        static void Postfix(ParticleSystemEventEffect __instance, ref BeatmapEventType ____colorEvent) {
            __instance.StartCoroutine(WaitThenStart(__instance, ____colorEvent));
        }

        private static IEnumerator WaitThenStart(ParticleSystemEventEffect __instance, BeatmapEventType ____colorEvent)
        {
            yield return new WaitForEndOfFrame();
            LightSwitchEventEffectExtensions.LSEStart(__instance, ____colorEvent);
        }

    }

}
