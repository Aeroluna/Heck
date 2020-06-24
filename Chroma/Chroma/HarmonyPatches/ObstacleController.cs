using Chroma.Events;
using Chroma.Settings;
using Chroma.Utils;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("Init")]
    internal class ObstacleControllerInit
    {
        internal static SimpleColorSO DefaultObstacleColorSO
        {
            get
            {
                if (defaultObstacleColor == null) defaultObstacleColor = Resources.FindObjectsOfTypeAll<ColorManager>().First().GetField<SimpleColorSO, ColorManager>("_obstaclesColor");
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

        private static void Prefix(ObstacleController __instance, ref SimpleColorSO ____color, ObstacleData obstacleData)
        {
            Color? c = null;

            // Technicolour
            if (ColourManager.TechnicolourBarriers && (ChromaConfig.TechnicolourWallsStyle != ColourManager.TechnicolourStyle.GRADIENT))
            {
                c = ColourManager.GetTechnicolour(true, Time.time + __instance.GetInstanceID(), ChromaConfig.TechnicolourWallsStyle);
            }

            // CustomObstacleColours
            List<TimedColor> colors = ChromaObstacleColourEvent.ObstacleColours.Where(n => n.time <= obstacleData.time).ToList();
            if (colors.Count > 0) c = colors.Last().color;

            // CustomJSONData _customData individual color override
            try
            {
                if (obstacleData is CustomObstacleData customData && ChromaBehaviour.LightingRegistered)
                {
                    dynamic dynData = customData.customData;

                    c = ChromaUtils.GetColorFromData(dynData) ?? c;
                }
            }
            catch (Exception e)
            {
                Logger.Log("INVALID _customData", Logger.Level.WARNING);
                Logger.Log(e);
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

        private static void Postfix(ObstacleController __instance)
        {
            if (!VFX.TechnicolourController.Instantiated()) return;
            VFX.TechnicolourController.Instance._obstacleControllers.Add(__instance);
        }
    }
}