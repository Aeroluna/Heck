using Chroma.Beatmap.Events;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("Init")]
    internal class ObstacleControllerInit
    {
        private static void Prefix(ObstacleController __instance, ref SimpleColorSO ____color, ref ObstacleData obstacleData)
        {
            Color? c = ColourManager.BarrierColour;

            // Technicolour
            if (ColourManager.TechnicolourBarriers && (ChromaConfig.TechnicolourWallsStyle != ColourManager.TechnicolourStyle.GRADIENT))
            {
                c = ColourManager.GetTechnicolour(true, Time.time + __instance.GetInstanceID(), ChromaConfig.TechnicolourWallsStyle);
            }

            // CustomObstacleColours
            if (ChromaObstacleColourEvent.CustomObstacleColours.Count > 0)
            {
                foreach (KeyValuePair<float, Color> d in ChromaObstacleColourEvent.CustomObstacleColours)
                {
                    if (d.Key <= obstacleData.time) c = d.Value;
                }
            }

            // CustomJSONData _customData individual color override
            try
            {
                if (obstacleData is CustomObstacleData customData && ChromaBehaviour.LightingRegistered)
                {
                    dynamic dynData = customData.customData;
                    if (dynData != null)
                    {
                        float? r = (float?)Trees.at(dynData, "_obstacleR");
                        float? g = (float?)Trees.at(dynData, "_obstacleG");
                        float? b = (float?)Trees.at(dynData, "_obstacleB");
                        if (r != null && g != null && b != null)
                        {
                            c = new Color(r.Value, g.Value, b.Value);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ChromaLogger.Log("INVALID _customData", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            if (c != null)
            {
                // create new SimpleColorSO, as to not overwrite the main color scheme
                ____color = ScriptableObject.CreateInstance<SimpleColorSO>();
                ____color.SetColor((Color)c);
            }
        }
    }

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("OnEnable")]
    internal class ObstacleControllerOnEnable
    {
        private static void Postfix(ObstacleController __instance)
        {
            if (!VFX.TechnicolourController.Instantiated()) return;
            VFX.TechnicolourController.Instance._stretchableObstacles.Add(__instance.GetPrivateField<StretchableObstacle>("_stretchableObstacle"));
        }
    }

    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("OnDisable")]
    internal class ObstacleControllerOnDisable
    {
        private static void Postfix(ObstacleController __instance)
        {
            if (!VFX.TechnicolourController.Instantiated()) return;
            VFX.TechnicolourController.Instance._stretchableObstacles.Remove(__instance.GetPrivateField<StretchableObstacle>("_stretchableObstacle"));
        }
    }
}