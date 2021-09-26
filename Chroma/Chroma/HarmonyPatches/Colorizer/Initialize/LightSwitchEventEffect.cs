namespace Chroma.HarmonyPatches
{
    using System.Collections;
    using Chroma.Colorizer;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch("Start")]
    internal static class LightSwitchEventEffectStart
    {
        private static void Postfix(LightSwitchEventEffect __instance, BeatmapEventType ____event)
        {
            __instance.StartCoroutine(WaitThenStart(__instance, ____event));
        }

        // For some reason, not waiting for end of frame causes the SO initializer to grab colors from the previous map, so whatever
        private static IEnumerator WaitThenStart(LightSwitchEventEffect instance, BeatmapEventType eventType)
        {
            yield return new WaitForEndOfFrame();
            new LightColorizer(instance, eventType);
        }
    }

    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch("OnDestroy")]
    internal static class LightSwitchEventEffectOnDestroy
    {
        [HarmonyPriority(Priority.Low)]
        private static void Postfix(BeatmapEventType ____event)
        {
            LightColorizer.Colorizers.Remove(____event);
        }
    }

    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class LightSwitchEventEffectSetLastEvent
    {
        private static void Prefix(BeatmapEventData beatmapEventData, BeatmapEventType ____event)
        {
            if (beatmapEventData.type == ____event)
            {
                // Who just looks through source code? you weirdo.....
                ____event.GetLightColorizer().PreviousEvent = beatmapEventData;
            }
        }
    }
}
