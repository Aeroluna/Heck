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
            // Technicolour
            if (ColourManager.TechnicolourBarriers && ((int)ChromaConfig.TechnicolourWallsStyle == 2)) {
                ColourManager.BarrierColour = ColourManager.GetTechnicolour(true, Time.time + __instance.GetInstanceID(), ColourManager.TechnicolourStyle.PURE_RANDOM);
            }

            // Save the _obstacleColor in case BarrierColor goes to Color.clear
            if (defaultObstacleColour == null) defaultObstacleColour = Resources.FindObjectsOfTypeAll<ColorManager>().First().GetPrivateField<SimpleColorSO>("_obstaclesColor").color;
            Color c = ColourManager.BarrierColour == Color.clear ? defaultObstacleColour.Value : ColourManager.BarrierColour;

            // CustomObstacleColours
            if (ChromaObstacleColourEvent.CustomObstacleColours.Count > 0) {
                foreach (KeyValuePair<float, Color> d in ChromaObstacleColourEvent.CustomObstacleColours) {
                    if (d.Key <= obstacleData.time) c = d.Value;
                }
            }

            // CustomJSONData _customData individual color override
            try {
                if (obstacleData is CustomObstacleData customData && ChromaUtils.CheckLightingEventRequirement()) {
                    dynamic dynData = customData.customData;
                    if (dynData != null) {
                        float? r = (float?)Trees.at(dynData, "_obstacleR");
                        float? g = (float?)Trees.at(dynData, "_obstacleG");
                        float? b = (float?)Trees.at(dynData, "_obstacleB");
                        if (r != null && g != null && b != null) {
                            c = new Color(r.Value, g.Value, b.Value);
                            //ChromaLogger.Log("Single barrier colour changed to " + c.ToString());
                        }
                    }
                }
            }
            catch (Exception e) {
                ChromaLogger.Log("INVALID _customData", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            if (c != Color.clear) ____color.SetColor(c);
        }
    }
}