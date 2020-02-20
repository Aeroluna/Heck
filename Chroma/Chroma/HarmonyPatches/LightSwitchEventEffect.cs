using Chroma.Events;
using Chroma.Extensions;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch("Start")]
    internal class LightSwitchEventEffectStart
    {
        private static void Postfix(LightSwitchEventEffect __instance, ref BeatmapEventType ____event)
        {
            __instance.StartCoroutine(WaitThenStart(__instance, ____event));
        }

        private static IEnumerator WaitThenStart(LightSwitchEventEffect __instance, BeatmapEventType ____event)
        {
            yield return new WaitForEndOfFrame();
            LightSwitchEventEffectExtensions.LSEStart(__instance, ____event);
        }
    }

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch("OnDestroy")]
    internal class LightSwitchEventEffectOnDestroy
    {
        private static void Postfix(LightSwitchEventEffect __instance, ref BeatmapEventType ____event)
        {
            LightSwitchEventEffectExtensions.LSEDestroy(__instance, ____event);
        }
    }

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch("SetColor")]
    internal class LightSwitchEventEffectSetColor
    {
        private static bool Prefix(LightSwitchEventEffect __instance, ref Color color)
        {
            if (LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.overrideLightWithIdActivation != null)
            {
                LightWithId[] lights = LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.overrideLightWithIdActivation;
                for (int i = 0; i < lights.Length; i++)
                {
                    lights[i].ColorWasSet(color);
                }

                return false;
            }

            return true;
        }
    }

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal class LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        public static void ResetRandom()
        {
            ChromaLogger.Log("Resetting techniLightRandom Random 408 (Light Switch)");
            techniLightRandom = new System.Random(408);
        }

        public static LightWithId[] overrideLightWithIdActivation = null;

        private static System.Random techniLightRandom = new System.Random(408);

        //0 = off
        //1 = blue on, 5 = red on
        //2 = blue flash, 6 = red flash
        //3 = blue fade, 7 = red fade
        private static bool Prefix(LightSwitchEventEffect __instance, ref BeatmapEventData beatmapEventData, ref BeatmapEventType ____event)
        {
            if (beatmapEventData.type != ____event) return true;

            // https://docs.google.com/spreadsheets/d/1vCTlDvx0ZW8NkkZBYW6ecvXaVRxDUKX7QIoah9PCp_c/edit#gid=0
            if (ColourManager.TechnicolourLights && (int)____event <= 4)
            {
                if (beatmapEventData.value > 0 && beatmapEventData.value <= 7)
                {
                    if (ChromaConfig.TechnicolourLightsStyle != ColourManager.TechnicolourStyle.GRADIENT)
                    { //0-4 are actual lighting events, we don't want to bother with anything else like ring spins or custom events
                        if (techniLightRandom.NextDouble() < ChromaConfig.TechnicolourLightsFrequency)
                        {
                            bool blue = beatmapEventData.value <= 3; //Blue events are 1, 2 and 3
                            switch (ChromaConfig.TechnicolourLightsGrouping)
                            {
                                case ColourManager.TechnicolourLightsGrouping.ISOLATED:
                                    VFX.MayhemEvent.ActivateTechnicolour(beatmapEventData, __instance);
                                    return false;

                                case ColourManager.TechnicolourLightsGrouping.ISOLATED_GROUP:
                                    __instance.SetLightingColourA(ColourManager.GetTechnicolour(!blue, beatmapEventData.time, ChromaConfig.TechnicolourLightsStyle));
                                    break;

                                case ColourManager.TechnicolourLightsGrouping.STANDARD:
                                default:
                                    Color? t = ColourManager.GetTechnicolour(!blue, beatmapEventData.time, ChromaConfig.TechnicolourLightsStyle);
                                    ColourManager.RecolourAllLights(blue ? null : t, blue ? t : null);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        // This is for fun gradient stuff
                        VFX.TechnicolourController.Instance._lightSwitchLastValue[__instance] = beatmapEventData.value;
                    }
                }
            }

            ColourLightSwitch(__instance, beatmapEventData, ____event);

            return true;
        }

        private static void SetOverrideLightWithIds(params LightWithId[] lights)
        {
            overrideLightWithIdActivation = lights;
        }

        private static void Postfix(LightSwitchEventEffect __instance)
        {
            overrideLightWithIdActivation = null;
        }

        public static void ColourLightSwitch(MonoBehaviour __monobehaviour, BeatmapEventData beatmapEventData, BeatmapEventType _event)
        {
            // We slap this puppy in a function so that ParticleSystemEventEffect can use it too
            Color? c = null;

            // CustomLightColours
            if (ChromaLightColourEvent.CustomLightColours.Count > 0)
            {
                Dictionary<float, Color> dictionaryID;
                if (ChromaLightColourEvent.CustomLightColours.TryGetValue(_event, out dictionaryID))
                {
                    foreach (KeyValuePair<float, Color> d in dictionaryID)
                    {
                        if (d.Key <= beatmapEventData.time) c = d.Value;
                    }
                }
            }

            // Ew gross legacy color events
            if (beatmapEventData.value >= ColourManager.RGB_INT_OFFSET) c = ColourManager.ColourFromInt(beatmapEventData.value);

            // CustomJSONData _customData individual override
            if (beatmapEventData is CustomBeatmapEventData customData)
            {
                dynamic dynData = customData.customData;
                if (__monobehaviour is LightSwitchEventEffect)
                {
                    LightSwitchEventEffect __instance = (LightSwitchEventEffect)__monobehaviour;

                    int? lightID = (int?)Trees.at(dynData, "_lightID");
                    if (lightID.HasValue)
                    {
                        LightWithId[] lights = __instance.GetLights();
                        if (lights.Length > lightID) SetOverrideLightWithIds(lights[lightID.Value]);
                    }

                    int? propID = (int?)Trees.at(dynData, "_propID");
                    if (propID.HasValue)
                    {
                        LightWithId[][] lights = __instance.GetLightsPropagationGrouped();
                        if (lights.Length > propID) SetOverrideLightWithIds(lights[propID.Value]);
                    }
                }

                if (ChromaBehaviour.LightingRegistered)
                {
                    if (__monobehaviour is LightSwitchEventEffect)
                    {
                        // GRADIENT
                        int? intid = (int?)Trees.at(dynData, "_event");
                        float? duration = (float?)Trees.at(dynData, "_duration");
                        float? initr = (float?)Trees.at(dynData, "_startR");
                        float? initg = (float?)Trees.at(dynData, "_startG");
                        float? initb = (float?)Trees.at(dynData, "_startB");
                        float? inita = (float?)Trees.at(dynData, "_startA");
                        float? endr = (float?)Trees.at(dynData, "_endR");
                        float? endg = (float?)Trees.at(dynData, "_endG");
                        float? endb = (float?)Trees.at(dynData, "_endB");
                        float? enda = (float?)Trees.at(dynData, "_endA");
                        if (intid.HasValue && duration.HasValue && initr.HasValue && initb.HasValue && enda.HasValue && endg.HasValue && endb.HasValue)
                        {
                            BeatmapEventType id = (BeatmapEventType)intid;
                            Color initc = new Color(initr.Value, initg.Value, initb.Value);
                            Color endc = new Color(endr.Value, endg.Value, endb.Value);
                            if (inita.HasValue) initc = initc.ColorWithAlpha(inita.Value);
                            if (enda.HasValue) endc = endc.ColorWithAlpha(enda.Value);

                            ChromaGradientEvent.AddGradient(id, initc, endc, customData.time, duration.Value);

                            c = initc;
                        }
                    }

                    // RGB
                    float? r = (float?)Trees.at(dynData, "_r");
                    float? g = (float?)Trees.at(dynData, "_g");
                    float? b = (float?)Trees.at(dynData, "_b");
                    float? a = (float?)Trees.at(dynData, "_a");
                    if (r.HasValue && g.HasValue && b.HasValue)
                    {
                        c = new Color(r.Value, g.Value, b.Value);
                        if (a.HasValue) c = c.Value.ColorWithAlpha(a.Value);

                        // Clear any active gradient
                        if (ChromaGradientEvent.CustomGradients.TryGetValue(_event, out ChromaGradientEvent gradient))
                        {
                            UnityEngine.Object.Destroy(gradient);
                            ChromaGradientEvent.CustomGradients.Remove(_event);
                        }
                    }
                }
            }

            if (c.HasValue) ColourManager.RecolourLight(ref __monobehaviour, c.Value, c.Value);
        }
    }
}