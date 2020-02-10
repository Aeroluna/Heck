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
using System.Collections;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("Start")]
    class ParticleSystemEventEffectStart {

        static void Postfix(ParticleSystemEventEffect __instance, ref BeatmapEventType ____colorEvent) {
            __instance.StartCoroutine(WaitThenStart(__instance, ____colorEvent));
        }

        private static IEnumerator WaitThenStart(ParticleSystemEventEffect __instance, BeatmapEventType ____colorEvent) {
            yield return new WaitForEndOfFrame();
            LightSwitchEventEffectExtensions.LSEStart(__instance, ____colorEvent);
        }

    }

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("OnDestroy")]
    class ParticleSystemEventEffectOnDestroy {

        static void Postfix(ParticleSystemEventEffect __instance, ref BeatmapEventType ____colorEvent) {
            LightSwitchEventEffectExtensions.LSEDestroy(__instance, ____colorEvent);
        }

    }

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    class ParticleSystemEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger {

        public static void ResetRandom() {
            ChromaLogger.Log("Resetting techniLightRandom Random 408 (Particles)");
            techniLightRandom = new System.Random(408);
        }

        private static System.Random techniLightRandom = new System.Random(408);

        static bool Prefix(ParticleSystemEventEffect __instance, ref BeatmapEventData beatmapEventData, ref BeatmapEventType ____colorEvent) {

            if (beatmapEventData.type != ____colorEvent) return true;

            if (beatmapEventData.value <= 7 && beatmapEventData.value >= 0) {
                if (VFX.TechnicolourController.Instantiated() && !VFX.TechnicolourController.Instance._particleSystemLastValue.TryGetValue(__instance, out int value)) {
                    VFX.TechnicolourController.Instance._particleSystemLastValue.Add(__instance, beatmapEventData.value);
                }
                else {
                    VFX.TechnicolourController.Instance._particleSystemLastValue[__instance] = beatmapEventData.value;
                }
            }

            MonoBehaviour __monobehaviour = __instance;
            Color? c = LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.CheckCJD(__monobehaviour, beatmapEventData, ____colorEvent);

            if (c != null) {
                ColourManager.RecolourLight(ref __monobehaviour, (Color)c, (Color)c);
            }

            try {

                // https://docs.google.com/spreadsheets/d/1vCTlDvx0ZW8NkkZBYW6ecvXaVRxDUKX7QIoah9PCp_c/edit#gid=0
                if (ColourManager.TechnicolourLights && (int)____colorEvent <= 4) { //0-4 are actual lighting events, we don't want to bother with anything else like ring spins or custom events
                    if (techniLightRandom.NextDouble() < ChromaConfig.TechnicolourLightsFrequency) {
                        if (beatmapEventData.value != 0 && (ChromaConfig.TechnicolourLightsGrouping == ColourManager.TechnicolourLightsGrouping.ISOLATED)) {
                            MayhemEvent.ParticleTechnicolour(beatmapEventData, __instance);
                            return false;
                        }
                    }
                }
            }
            catch (Exception e) {
                ChromaLogger.Log("Exception handling technicolour lights!", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            return LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ActivateLegacyEvent(__instance, ref beatmapEventData, ref ____colorEvent);
        }

    }

}
