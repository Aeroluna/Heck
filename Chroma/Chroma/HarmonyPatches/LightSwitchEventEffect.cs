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

        private static IEnumerator WaitThenStart(LightSwitchEventEffect instance, BeatmapEventType eventType)
        {
            yield return new WaitForEndOfFrame();
            LightColorizer.LSEStart(instance, eventType);
        }
    }

    [ChromaPatch(typeof(LightSwitchEventEffect))]
    [ChromaPatch("SetColor")]
    internal static class LightSwitchEventEffectSetColor
    {
        private static bool Prefix(Color color)
        {
            if (LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.OverrideLightWithIdActivation != null)
            {
                ILightWithId[] lights = LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.OverrideLightWithIdActivation;
                for (int i = 0; i < lights.Length; i++)
                {
                    lights[i].ColorWasSet(color);
                }

                return false;
            }

            return true;
        }
    }

    [ChromaPatch(typeof(LightSwitchEventEffect))]
    [ChromaPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        internal static ILightWithId[] OverrideLightWithIdActivation { get; set; }

        // 0 = off
        // 1 = blue on, 5 = red on
        // 2 = blue flash, 6 = red flash
        // 3 = blue fade, 7 = red fade
        private static void Prefix(LightSwitchEventEffect __instance, BeatmapEventData beatmapEventData, BeatmapEventType ____event)
        {
            if (beatmapEventData.type == ____event)
            {
                LightColorManager.ColorLightSwitch(__instance, beatmapEventData);
            }
        }

        private static void Postfix()
        {
            OverrideLightWithIdActivation = null;
        }
    }
}
