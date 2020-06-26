namespace Chroma.HarmonyPatches
{
    using System.Linq;
    using Chroma.Utils;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using IPA.Utilities;
    using UnityEngine;

    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("Init")]
    internal class ObstacleControllerInit
    {
        private static SimpleColorSO _customObstacleColor;
        private static SimpleColorSO _defaultObstacleColor;

        internal static SimpleColorSO DefaultObstacleColorSO
        {
            get
            {
                if (_defaultObstacleColor == null)
                {
                    _defaultObstacleColor = Resources.FindObjectsOfTypeAll<ColorManager>().First().GetField<SimpleColorSO, ColorManager>("_obstaclesColor");
                }

                return _defaultObstacleColor;
            }
        }

        internal static SimpleColorSO CustomObstacleColorSO
        {
            get
            {
                if (_customObstacleColor == null)
                {
                    _customObstacleColor = ScriptableObject.CreateInstance<SimpleColorSO>();
                }

                return _customObstacleColor;
            }
        }

        internal static void ClearObstacleColors()
        {
            _defaultObstacleColor = null;
            Object.Destroy(_customObstacleColor);
            _customObstacleColor = null;
        }

#pragma warning disable SA1313
        private static void Prefix(ObstacleController __instance, ref SimpleColorSO ____color, ObstacleData obstacleData)
#pragma warning restore SA1313
        {
            Color? c = null;

            // CustomJSONData _customData individual color override
            if (obstacleData is CustomObstacleData customData && ChromaBehaviour.LightingRegistered)
            {
                dynamic dynData = customData.customData;

                c = ChromaUtils.GetColorFromData(dynData) ?? c;
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
}
