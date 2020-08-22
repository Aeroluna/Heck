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
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(LightSwitchEventEffect __instance, BeatmapEventType ____event)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
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
                LightWithId[] lights = LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.OverrideLightWithIdActivation;
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
        internal static LightWithId[] OverrideLightWithIdActivation { get; set; }

        // 0 = off
        // 1 = blue on, 5 = red on
        // 2 = blue flash, 6 = red flash
        // 3 = blue fade, 7 = red fade
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Prefix(LightSwitchEventEffect __instance, BeatmapEventData beatmapEventData, BeatmapEventType ____event)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
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
