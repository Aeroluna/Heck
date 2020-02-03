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
    [HarmonyPatch(typeof(ParticleSystemEventEffect))]
    [HarmonyPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    class ParticleSystemEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger {

        public static void ResetRandom() {
            ChromaLogger.Log("Resetting techniLightRandom Random 408 (Particles)");
            techniLightRandom = new System.Random(408);
        }

        private static System.Random techniLightRandom = new System.Random(408);

        static bool Prefix(ParticleSystemEventEffect __instance, ref BeatmapEventData beatmapEventData, ref BeatmapEventType ____colorEvent) {
            try {

                // https://docs.google.com/spreadsheets/d/1vCTlDvx0ZW8NkkZBYW6ecvXaVRxDUKX7QIoah9PCp_c/edit#gid=0
                if (ColourManager.TechnicolourLights && (int)____colorEvent <= 4) { //0-4 are actual lighting events, we don't want to bother with anything else like ring spins or custom events
                    if (techniLightRandom.NextDouble() < ChromaConfig.TechnicolourLightsFrequency) {
                        if (beatmapEventData.value != 0 && (ChromaConfig.TechnicolourLightsGrouping == ColourManager.TechnicolourLightsGrouping.ISOLATED)) {
                            MayhemEvent.ParticleTechnicolour(beatmapEventData, __instance);
                            return false;
                        }
                    }
                }
            }
            catch (Exception e) {
                ChromaLogger.Log("Exception handling technicolour lights!", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            return LightSwitchEventEffectHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger.ActivateLegacyEvent(__instance, ref beatmapEventData, ref ____colorEvent);
        }

    }

}
