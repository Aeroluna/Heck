using Chroma.Beatmap.ChromaEvents;
using Chroma.Beatmap.Events;
using Chroma.Beatmap.Z_Testing.ChromaEvents;
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
            // CustomLightColours
            if (ChromaLightColourEvent.CustomLightColours.Count > 0) {
                Dictionary<float, Color> dictionaryID;
                if (ChromaLightColourEvent.CustomLightColours.TryGetValue(__instance.LightsID - 1, out dictionaryID)) {
                    foreach (KeyValuePair<float, Color> d in dictionaryID) {
                        if (d.Key <= beatmapEventData.time) {
                            MonoBehaviour __monobehaviour = __instance;
                            ColourManager.RecolourLight(ref __monobehaviour, d.Value, d.Value);
                        }
                    }
                }
            }

            try {

                if (beatmapEventData.type == ____event) {
                    if (beatmapEventData is CustomBeatmapEventData customData) {
                        dynamic dynData = customData.customData;
                        if (dynData != null)
                        {
                            long? lightID = Trees.at(dynData, "_lightID");
                            if (lightID != null)
                            {
                                LightWithId[] lights = __instance.GetLights();
                                if (lights.Length > lightID) SetOverrideLightWithIds(lights[(int)lightID]);
                            }

                            long? propID = Trees.at(dynData, "_propID");
                            if (propID != null)
                            {
                                LightWithId[][] lights = __instance.GetLightsPropagationGrouped();
                                if (lights.Length > propID) SetOverrideLightWithIds(lights[(int)propID]);
                            }

                            if (Utils.ChromaUtils.CheckLightingEventRequirement()) {
                                float? r = (float?)Trees.at(dynData, "r");
                                float? g = (float?)Trees.at(dynData, "g");
                                float? b = (float?)Trees.at(dynData, "b");
                                if (r != null && g != null && b != null) {
                                    Color c = new Color((float)r, (float)g, (float)b);
                                    float? a = (float?)Trees.at(dynData, "a");
                                    if (a != null) c = c.ColorWithAlpha((float)a);
                                    MonoBehaviour __monobehaviour = __instance;
                                    ColourManager.RecolourLight(ref __monobehaviour, c, c);
                                }
                            }
                        }
                    }
                }

            } catch (Exception e) {
                ChromaLogger.Log("INVALID _customData", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            try {

                // https://docs.google.com/spreadsheets/d/1vCTlDvx0ZW8NkkZBYW6ecvXaVRxDUKX7QIoah9PCp_c/edit#gid=0
                if (ColourManager.TechnicolourLights && (int)____event <= 4) { //0-4 are actual lighting events, we don't want to bother with anything else like ring spins or custom events
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
                                default:
                                    Color c = ColourManager.GetTechnicolour(!blue, beatmapEventData.time, ChromaConfig.TechnicolourLightsStyle);
                                    ColourManager.RecolourAllLights(blue ? Color.clear : c, blue ? c : Color.clear);
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
