namespace Chroma.HarmonyPatches
{
    using System.Collections;
    using Chroma.Extensions;
    using UnityEngine;

    [ChromaPatch(typeof(ParticleSystemEventEffect))]
    [ChromaPatch("Start")]
    internal class ParticleSystemEventEffectStart
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(ParticleSystemEventEffect __instance, BeatmapEventType ____colorEvent)
#pragma warning restore SA1313
        {
            __instance.StartCoroutine(WaitThenStart(__instance, ____colorEvent));
        }

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static IEnumerator WaitThenStart(ParticleSystemEventEffect __instance, BeatmapEventType ____colorEvent)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            yield return new WaitForEndOfFrame();
            LightSwitchEventEffectExtensions.LSEStart(__instance, ____colorEvent);
        }
    }

    [ChromaPatch(typeof(ParticleSystemEventEffect))]
    [ChromaPatch("OnDestroy")]
    internal class ParticleSystemEventEffectOnDestroy
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(ParticleSystemEventEffect __instance, BeatmapEventType ____colorEvent)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            LightSwitchEventEffectExtensions.LSEDestroy(__instance, ____colorEvent);
        }
    }

    [ChromaPatch(typeof(ParticleSystemEventEffect))]
    [ChromaPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal class ParticleSystemEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static bool Prefix(ParticleSystemEventEffect __instance, BeatmapEventData beatmapEventData, BeatmapEventType ____colorEvent)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            if (beatmapEventData.type != ____colorEvent)
            {
                return true;
            }

            LightColorManager.ColorLightSwitch(__instance, beatmapEventData, ____colorEvent);

            return true;
        }
    }
}
