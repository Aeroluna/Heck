using Chroma.Beatmap.ChromaEvents;
using Chroma.Beatmap.Events;
using Chroma.Beatmap.Events.Legacy;
using Chroma.Extensions;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using IPA.Utilities;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    class LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger {

        public static void ResetRandom() {
            ChromaLogger.Log("Resetting techniLightRandom Random 408 (Light Switch)");
            techniLightRandom = new System.Random(408);
        }

        public static LightWithId[] overrideLightWithIdActivation = null;

        private static System.Random techniLightRandom = new System.Random(408);

        //0 = off
        //1 = blue on, 5 = red on
        //2 = blue flash, 6 = red flash
        //3 = blue fade, 7 = red fade
        static bool Prefix(LightSwitchEventEffect __instance, ref BeatmapEventData beatmapEventData, ref BeatmapEventType ____event) {

            if (beatmapEventData.type != ____event) return true;

            if (beatmapEventData.value <= 7 && beatmapEventData.value >= 0) {
                if (VFX.TechnicolourController.Instantiated() && !VFX.TechnicolourController.Instance._lightSwitchLastValue.TryGetValue(__instance, out int value)) {
                    VFX.TechnicolourController.Instance._lightSwitchLastValue.Add(__instance, beatmapEventData.value);
                }
                else {
                    VFX.TechnicolourController.Instance._lightSwitchLastValue[__instance] = beatmapEventData.value;
                }
            }

            MonoBehaviour __monobehaviour = __instance;
            Color? c = CheckCJD(__monobehaviour, beatmapEventData, ____event);

            if (c != null) {
                ColourManager.RecolourLight(ref __monobehaviour, (Color)c, (Color)c);
            }

            try {

                // https://docs.google.com/spreadsheets/d/1vCTlDvx0ZW8NkkZBYW6ecvXaVRxDUKX7QIoah9PCp_c/edit#gid=0
                if (ColourManager.TechnicolourLights && ChromaConfig.TechnicolourLightsStyle != ColourManager.TechnicolourStyle.GRADIENT && (int)____event <= 4) { //0-4 are actual lighting events, we don't want to bother with anything else like ring spins or custom events
                                                                               //System.Random noteRandom = new System.Random(Mathf.FloorToInt(beatmapEventData.time * 408));
                    if (techniLightRandom.NextDouble() < ChromaConfig.TechnicolourLightsFrequency) {
                        if (beatmapEventData.value > 0 && beatmapEventData.value <= 7) {
                            bool blue = beatmapEventData.value <= 3; //Blue events are 1, 2 and 3
                            switch (ChromaConfig.TechnicolourLightsGrouping) {
                                case ColourManager.TechnicolourLightsGrouping.ISOLATED:
                                    MayhemEvent.ActivateTechnicolour(beatmapEventData, __instance);
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
                }

            } catch (Exception e) {
                ChromaLogger.Log("Exception handling technicolour lights!", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            return ActivateLegacyEvent(__instance, ref beatmapEventData, ref ____event);
        }

        private static void SetOverrideLightWithIds(params LightWithId[] lights) {
            overrideLightWithIdActivation = lights;
        }

        static void Postfix(LightSwitchEventEffect __instance) {
            overrideLightWithIdActivation = null;
        }

        public static Color? CheckCJD(MonoBehaviour __monobehaviour, BeatmapEventData beatmapEventData, BeatmapEventType _event) {
            Color? c = null;

            // CustomLightColours
            if (ChromaLightColourEvent.CustomLightColours.Count > 0) {
                Dictionary<float, Color> dictionaryID;
                if (ChromaLightColourEvent.CustomLightColours.TryGetValue(_event, out dictionaryID)) {
                    foreach (KeyValuePair<float, Color> d in dictionaryID) {
                        if (d.Key <= beatmapEventData.time) {
                            c = d.Value;
                        }
                    }
                }
            }
            
            try {

                if (beatmapEventData is CustomBeatmapEventData customData) {
                    dynamic dynData = customData.customData;
                    if (dynData != null) {
                        if (__monobehaviour is LightSwitchEventEffect) {
                            LightSwitchEventEffect __instance = (LightSwitchEventEffect)__monobehaviour;

                            long? lightID = Trees.at(dynData, "_lightID");
                            if (lightID != null) {
                                LightWithId[] lights = __instance.GetLights();
                                if (lights.Length > lightID) SetOverrideLightWithIds(lights[(int)lightID]);
                            }

                            long? propID = Trees.at(dynData, "_propID");
                            if (propID != null) {
                                LightWithId[][] lights = __instance.GetLightsPropagationGrouped();
                                if (lights.Length > propID) SetOverrideLightWithIds(lights[(int)propID]);
                            }
                        }
                        
                        if (ChromaBehaviour.LightingRegistered) {
                            if (__monobehaviour is LightSwitchEventEffect) {
                                // GRADIENT
                                int? intid = (int?)Trees.at(dynData, "_lightsID");
                                float? duration = (float?)Trees.at(dynData, "_duration");
                                float? initr = (float?)Trees.at(dynData, "_startR");
                                float? initg = (float?)Trees.at(dynData, "_startG");
                                float? initb = (float?)Trees.at(dynData, "_startB");
                                float? inita = (float?)Trees.at(dynData, "_startA");
                                float? endr = (float?)Trees.at(dynData, "_endR");
                                float? endg = (float?)Trees.at(dynData, "_endG");
                                float? endb = (float?)Trees.at(dynData, "_endB");
                                float? enda = (float?)Trees.at(dynData, "_endA");
                                if (intid != null && duration != null && initr != null && initb != null && enda != null && endg != null && endb != null) {
                                    BeatmapEventType id = (BeatmapEventType)intid;
                                    Color initc = new Color((float)initr, (float)initg, (float)initb);
                                    Color endc = new Color((float)endr, (float)endg, (float)endb);
                                    if (inita != null) initc = initc.ColorWithAlpha((float)inita);
                                    if (enda != null) endc = endc.ColorWithAlpha((float)enda);

                                    ChromaGradientEvent.AddGradient(id, initc, endc, customData.time, (float)duration);

                                    return initc;
                                }
                            }

                            // RGB
                            float? r = (float?)Trees.at(dynData, "r");
                            float? g = (float?)Trees.at(dynData, "g");
                            float? b = (float?)Trees.at(dynData, "b");
                            if (r != null && g != null && b != null) {
                                Color d = new Color((float)r, (float)g, (float)b);
                                float? a = (float?)Trees.at(dynData, "a");
                                if (a != null) d = d.ColorWithAlpha((float)a);
                                c = d;

                                // Clear any active gradient
                                if (ChromaGradientEvent.CustomGradients.TryGetValue(_event, out ChromaGradientEvent gradient)) {
                                    UnityEngine.Object.Destroy(gradient);
                                    ChromaGradientEvent.CustomGradients.Remove(_event);
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception e) {
                ChromaLogger.Log("INVALID _customData", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }
            
            return c;
        }

        public static bool ActivateLegacyEvent(MonoBehaviour __instance, ref BeatmapEventData beatmapEventData, ref BeatmapEventType ____event) {
            try {

                //if (ChromaEvent.SimpleEventActivate(__instance, ref beatmapEventData, ref ____event)) return false;

                if (beatmapEventData.type == ____event) {
                    ChromaEvent customEvent = ChromaEvent.GetChromaEvent(beatmapEventData);
                    if (customEvent != null) {
                        if (customEvent.RequiresColourEventsEnabled && !ChromaConfig.CustomColourEventsEnabled) return false;
                        customEvent.Activate(ref __instance, ref beatmapEventData, ref ____event);
                        return false;
                    }
                }

            }
            catch (Exception e) {
                ChromaLogger.Log("Exception handling legacy event!", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }
            return true;
        }

    }

}
