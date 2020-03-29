using Chroma.Extensions;
using Chroma.Settings;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
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

    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("OnDestroy")]
    internal class ParticleSystemEventEffectOnDestroy
    {
        private static void Postfix(ParticleSystemEventEffect __instance, ref BeatmapEventType ____colorEvent)
        {
            LightSwitchEventEffectExtensions.LSEDestroy(__instance, ____colorEvent);
        }
    }

    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal class ParticleSystemEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        internal static void ResetRandom()
        {
            techniLightRandom = new System.Random(408);
        }

        private static System.Random techniLightRandom = new System.Random(408);

        private static bool Prefix(ParticleSystemEventEffect __instance, ref BeatmapEventData beatmapEventData, ref BeatmapEventType ____colorEvent)
        {
            if (beatmapEventData.type != ____colorEvent) return true;

            if (ColourManager.TechnicolourLights && (int)____colorEvent <= 4)
            {
                if (beatmapEventData.value > 0 && beatmapEventData.value <= 7)
                {
                    if (ChromaConfig.TechnicolourLightsGrouping == ColourManager.TechnicolourLightsGrouping.ISOLATED &&
                        techniLightRandom.NextDouble() < ChromaConfig.TechnicolourLightsFrequency)
                    {
                        // ParticleSystem need only worry about mayhem
                        VFX.MayhemEvent.ParticleTechnicolour(beatmapEventData, __instance);
                        return false;
                    }
                }
            }

            LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ColourLightSwitch(__instance, beatmapEventData, ____colorEvent);

            return true;
        }
    }
}