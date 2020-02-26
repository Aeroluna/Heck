using Chroma.Extensions;
using Chroma.Settings;
using Harmony;
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
                    else if (ChromaConfig.TechnicolourLightsStyle != ColourManager.TechnicolourStyle.GRADIENT)
                    {
                        // This is for fun gradient stuff
                        VFX.TechnicolourController.Instance._particleSystemLastValue[__instance] = beatmapEventData.value;
                    }
                }
            }

            LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ColourLightSwitch(__instance, beatmapEventData, ____colorEvent);

            return true;
        }
    }
}