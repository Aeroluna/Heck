namespace Chroma.HarmonyPatches
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
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
        private static bool Prefix(LightSwitchEventEffect __instance, BeatmapEventType ____event, Color color)
        {
            if (LightColorManager.LightIDOverride != null)
            {
                List<ILightWithId> lights = __instance.GetLights();
                int type = (int)____event;
                IEnumerable<int> newIds = LightColorManager.LightIDOverride.Select(n => LightIDTableManager.GetActiveTableValue(type, n) ?? n);
                foreach (int id in newIds)
                {
                    ILightWithId lightWithId = lights.ElementAtOrDefault(id);
                    if (lightWithId != null)
                    {
                        if (lightWithId.isRegistered)
                        {
                            lightWithId.ColorWasSet(color);
                        }
                    }
                    else
                    {
                        ChromaLogger.Log($"Type [{type}] does not contain id [{id}].", IPA.Logging.Logger.Level.Warning);
                    }
                }

                LightColorManager.LightIDOverride = null;

                return false;
            }

            // Legacy Prop Id stuff
            if (LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.LegacyLightOverride != null)
            {
                ILightWithId[] lights = LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.LegacyLightOverride;
                for (int i = 0; i < lights.Length; i++)
                {
                    lights[i].ColorWasSet(color);
                }

                LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.LegacyLightOverride = null;

                return false;
            }

            return true;
        }
    }

    [ChromaPatch(typeof(LightSwitchEventEffect))]
    [ChromaPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        internal static ILightWithId[] LegacyLightOverride { get; set; }

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
    }

    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class LightSwitchEventEffectSetLastEvent
    {
        private static void Prefix(LightSwitchEventEffect __instance, BeatmapEventData beatmapEventData, BeatmapEventType ____event)
        {
            if (beatmapEventData.type == ____event)
            {
                __instance.SetLastValue(beatmapEventData.value);
            }
        }
    }
}
