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
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(ParticleSystemEventEffect __instance, BeatmapEventType ____colorEvent)
#pragma warning restore SA1313
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
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Prefix(ParticleSystemEventEffect __instance, BeatmapEventData beatmapEventData, BeatmapEventType ____colorEvent)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            if (beatmapEventData.type == ____colorEvent)
            {
                LightColorManager.ColorLightSwitch(__instance, beatmapEventData);
            }
        }
    }
}
