using Chroma.Extensions;
using Chroma.Settings;
using Harmony;
using System;
using System.Collections;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("Start")]
    internal class ParticleSystemEventEffectStart
    {
        private static void Postfix(ParticleSystemEventEffect __instance, ref BeatmapEventType ____colorEvent)
        {
            __instance.StartCoroutine(WaitThenStart(__instance, ____colorEvent));
        }

        private static IEnumerator WaitThenStart(ParticleSystemEventEffect __instance, BeatmapEventType ____colorEvent)
        {
            yield return new WaitForEndOfFrame();
            LightSwitchEventEffectExtensions.LSEStart(__instance, ____colorEvent);
        }
    }

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("OnDestroy")]
    internal class ParticleSystemEventEffectOnDestroy
    {
        private static void Postfix(ParticleSystemEventEffect __instance, ref BeatmapEventType ____colorEvent)
        {
            LightSwitchEventEffectExtensions.LSEDestroy(__instance, ____colorEvent);
        }
    }

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal class ParticleSystemEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        public static void ResetRandom()
        {
            ChromaLogger.Log("Resetting techniLightRandom Random 408 (Particles)");
            techniLightRandom = new System.Random(408);
        }

        private static System.Random techniLightRandom = new System.Random(408);

        private static bool Prefix(ParticleSystemEventEffect __instance, ref BeatmapEventData beatmapEventData, ref BeatmapEventType ____colorEvent)
        {
            if (beatmapEventData.type != ____colorEvent) return true;

            if (beatmapEventData.value <= 7 && beatmapEventData.value >= 0)
            {
                if (VFX.TechnicolourController.Instantiated() && !VFX.TechnicolourController.Instance._particleSystemLastValue.TryGetValue(__instance, out int value))
                {
                    VFX.TechnicolourController.Instance._particleSystemLastValue.Add(__instance, beatmapEventData.value);
                }
                else
                {
                    VFX.TechnicolourController.Instance._particleSystemLastValue[__instance] = beatmapEventData.value;
                }
            }

            MonoBehaviour __monobehaviour = __instance;
            Color? c = LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.CheckCJD(__monobehaviour, beatmapEventData, ____colorEvent);

            if (c.HasValue)
            {
                ColourManager.RecolourLight(ref __monobehaviour, c.Value, c.Value);
            }

            try
            {
                // https://docs.google.com/spreadsheets/d/1vCTlDvx0ZW8NkkZBYW6ecvXaVRxDUKX7QIoah9PCp_c/edit#gid=0
                if (ColourManager.TechnicolourLights && (int)____colorEvent <= 4)
                { //0-4 are actual lighting events, we don't want to bother with anything else like ring spins or custom events
                    if (techniLightRandom.NextDouble() < ChromaConfig.TechnicolourLightsFrequency)
                    {
                        if (beatmapEventData.value != 0 && (ChromaConfig.TechnicolourLightsGrouping == ColourManager.TechnicolourLightsGrouping.ISOLATED))
                        {
                            VFX.MayhemEvent.ParticleTechnicolour(beatmapEventData, __instance);
                            return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ChromaLogger.Log("Exception handling technicolour lights!", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            return LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ActivateLegacyEvent(__instance, ref beatmapEventData, ref ____colorEvent);
        }
    }
}