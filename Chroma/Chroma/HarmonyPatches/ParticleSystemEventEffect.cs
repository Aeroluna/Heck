namespace Chroma.HarmonyPatches
{
    using System.Collections;
    using Chroma.Colorizer;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("Start")]
    internal static class ParticleSystemEventEffectStart
    {
        private static void Postfix(ParticleSystemEventEffect __instance, BeatmapEventType ____colorEvent)
        {
            __instance.StartCoroutine(WaitThenStart(__instance, ____colorEvent));
        }

        private static IEnumerator WaitThenStart(ParticleSystemEventEffect instance, BeatmapEventType eventType)
        {
            yield return new WaitForEndOfFrame();
            LightColorizer.LSEStart(instance, eventType);
        }
    }

    [ChromaPatch(typeof(ParticleSystemEventEffect))]
    [ChromaPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class ParticleSystemEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        private static void Prefix(ParticleSystemEventEffect __instance, BeatmapEventData beatmapEventData, BeatmapEventType ____colorEvent)
        {
            if (beatmapEventData.type == ____colorEvent)
            {
                LightColorManager.ColorLightSwitch(__instance, beatmapEventData);
            }
        }
    }

    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class ParticleSystemEventEffectSetLastEvent
    {
        private static void Prefix(ParticleSystemEventEffect __instance, BeatmapEventData beatmapEventData, BeatmapEventType ____colorEvent)
        {
            if (beatmapEventData.type == ____colorEvent)
            {
                __instance.SetLastValue(beatmapEventData.value);
            }
        }
    }
}
