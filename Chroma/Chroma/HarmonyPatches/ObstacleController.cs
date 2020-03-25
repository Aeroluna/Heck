using Chroma.Events;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("Init")]
    internal class ObstacleControllerInit
    {
        internal static SimpleColorSO DefaultObstacleColorSO
        {
            get
            {
                if (defaultObstacleColor == null) defaultObstacleColor = Resources.FindObjectsOfTypeAll<ColorManager>().First().GetPrivateField<SimpleColorSO>("_obstaclesColor");
                return defaultObstacleColor;
            }
        }

        private static SimpleColorSO defaultObstacleColor;

        internal static SimpleColorSO CustomObstacleColorSO
        {
            get
            {
                if (customObstacleColor == null) customObstacleColor = ScriptableObject.CreateInstance<SimpleColorSO>();
                return customObstacleColor;
            }
        }

        private static SimpleColorSO customObstacleColor;

        internal static void ClearObstacleColors()
        {
            defaultObstacleColor = null;
            UnityEngine.Object.Destroy(customObstacleColor);
            customObstacleColor = null;
        }

        private static void Prefix(ObstacleController __instance, ref SimpleColorSO ____color, ref ObstacleData obstacleData)
        {
            Color? c = null;

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

                    List<object> color = Trees.at(dynData, "_color");
                    if (color != null)
                    {
                        float r = Convert.ToSingle(color[0]);
                        float g = Convert.ToSingle(color[1]);
                        float b = Convert.ToSingle(color[2]);

                        c = new Color(r, g, b);
                        if (color.Count > 3) c = c.Value.ColorWithAlpha(Convert.ToSingle(color[3]));
                    }
                }
            }
            catch (Exception e)
            {
                ChromaLogger.Log("INVALID _customData", ChromaLogger.Level.WARNING);
                ChromaLogger.Log(e);
            }

            if (c.HasValue)
            {
                ____color = CustomObstacleColorSO;
                ____color.SetColor(c.Value);
            }
            else
            {
                ____color = DefaultObstacleColorSO;
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