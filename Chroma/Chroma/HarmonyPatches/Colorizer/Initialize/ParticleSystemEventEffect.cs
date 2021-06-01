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
            new ParticleColorizer(instance, eventType);
        }
    }

    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("OnDestroy")]
    internal static class ParticleSystemEventEffectOnDestroy
    {
        [HarmonyPriority(Priority.Low)]
        private static void Postfix(BeatmapEventType ____colorEvent)
        {
            if (____colorEvent.TryGetParticleColorizer(out ParticleColorizer particleColorizer))
            {
                particleColorizer.UnsubscribeEvent();
            }

            ParticleColorizer.Colorizers.Remove(____colorEvent);
        }
    }

    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class ParticleSystemEventEffectSetLastEvent
    {
        private static void Prefix(BeatmapEventData beatmapEventData, BeatmapEventType ____colorEvent)
        {
            if (beatmapEventData.type == ____colorEvent && ParticleColorizer.Colorizers.TryGetValue(____colorEvent, out ParticleColorizer particleColorizer))
            {
                particleColorizer.PreviousValue = beatmapEventData.value;
            }
        }
    }
}
