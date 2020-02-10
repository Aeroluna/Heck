using Chroma.Settings;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using IPA.Utilities;
using Chroma.Utils;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Chroma.Beatmap.Events;

namespace Chroma.HarmonyPatches {

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("Init")]
    class ObstacleControllerInit {

        internal static Color? defaultObstacleColour;
        static void Prefix(ObstacleController __instance, ref SimpleColorSO ____color, ref ObstacleData obstacleData) {
            Color? c = ColourManager.BarrierColour;

            // Technicolour
            if (ColourManager.TechnicolourBarriers && (ChromaConfig.TechnicolourWallsStyle != ColourManager.TechnicolourStyle.GRADIENT)) {
                c = ColourManager.GetTechnicolour(true, Time.time + __instance.GetInstanceID(), ChromaConfig.TechnicolourWallsStyle);
            }

            // CustomObstacleColours
            if (ChromaObstacleColourEvent.CustomObstacleColours.Count > 0) {
                foreach (KeyValuePair<float, Color> d in ChromaObstacleColourEvent.CustomObstacleColours) {
                    if (d.Key <= obstacleData.time) c = d.Value;
                }
            }

            // CustomJSONData _customData individual color override
            try {
                if (obstacleData is CustomObstacleData customData && ChromaBehaviour.LightingRegistered) {
                    dynamic dynData = customData.customData;
                    if (dynData != null) {
                        float? r = (float?)Trees.at(dynData, "_obstacleR");
                        float? g = (float?)Trees.at(dynData, "_obstacleG");
                        float? b = (float?)Trees.at(dynData, "_obstacleB");
                        if (r != null && g != null && b != null) {
                            c = new Color(r.Value, g.Value, b.Value);
                        }
                    }
                }
            }
            catch (Exception e) {
                ChromaLogger.Log("INVALID _customData", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            if (c != null) ____color.SetColor((Color)c);
        }
    }

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("OnEnable")]
    class ObstacleControllerOnEnable {
        static void Postfix(ObstacleController __instance) {
            if (!VFX.TechnicolourController.Instantiated()) return;
            VFX.TechnicolourController.Instance._stretchableObstacles.Add(__instance.GetPrivateField<StretchableObstacle>("_stretchableObstacle"));
        }
    }

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("OnDisable")]
    class ObstacleControllerOnDisable {
        static void Postfix(ObstacleController __instance) {
            if (!VFX.TechnicolourController.Instantiated()) return;
            VFX.TechnicolourController.Instance._stretchableObstacles.Remove(__instance.GetPrivateField<StretchableObstacle>("_stretchableObstacle"));
        }
    }
}