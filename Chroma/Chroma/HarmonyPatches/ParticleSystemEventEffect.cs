namespace Chroma.HarmonyPatches
{
    using System.Collections;
    using Chroma.Extensions;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("Start")]
    internal class ParticleSystemEventEffectStart
    {
#pragma warning disable SA1313
        private static void Postfix(ParticleSystemEventEffect __instance, BeatmapEventType ____colorEvent)
#pragma warning restore SA1313
        {
            if (ChromaBehaviour.LightingRegistered || ChromaBehaviour.LegacyOverride)
            {
                __instance.StartCoroutine(WaitThenStart(__instance, ____colorEvent));
            }
        }

#pragma warning disable SA1313
        private static IEnumerator WaitThenStart(ParticleSystemEventEffect __instance, BeatmapEventType ____colorEvent)
#pragma warning restore SA1313
        {
            yield return new WaitForEndOfFrame();
            LightSwitchEventEffectExtensions.LSEStart(__instance, ____colorEvent);
        }
    }

    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("OnDestroy")]
    internal class ParticleSystemEventEffectOnDestroy
    {
#pragma warning disable SA1313
        private static void Postfix(ParticleSystemEventEffect __instance, BeatmapEventType ____colorEvent)
#pragma warning restore SA1313
        {
            LightSwitchEventEffectExtensions.LSEDestroy(__instance, ____colorEvent);
        }
    }

    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal class ParticleSystemEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
#pragma warning disable SA1313
        private static bool Prefix(ParticleSystemEventEffect __instance, BeatmapEventData beatmapEventData, BeatmapEventType ____colorEvent)
#pragma warning restore SA1313
        {
            if (beatmapEventData.type != ____colorEvent)
            {
                return true;
            }

            LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ColorLightSwitch(__instance, beatmapEventData, ____colorEvent);

            return true;
        }
    }
}
