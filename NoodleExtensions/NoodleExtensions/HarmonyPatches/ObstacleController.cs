using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPriority(Priority.Normal)]
    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("Init")]
    internal class ObstacleControllerInit
    {
        public static void Postfix(ref ObstacleController __instance, ObstacleData obstacleData, Vector3 startPos,
            Vector3 midPos, Vector3 endPos, float move1Duration, float move2Duration, float startTimeOffset, float singleLineWidth,
            ref bool ____initialized, ref Vector3 ____startPos, ref Vector3 ____endPos, ref Vector3 ____midPos, ref StretchableObstacle ____stretchableObstacle, ref Bounds ____bounds, ref SimpleColorSO ____color, ref float height)
        {
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                if (dynData != null)
                {
                    float? _startRow = (float?)Trees.at(dynData, "_startRow"); // TODO: move to different patch
                    float? _startHeight = (float?)Trees.at(dynData, "_startHeight");
                    float? _height = (float?)Trees.at(dynData, "_height");
                    float? _width = (float?)Trees.at(dynData, "_width");

                    if (_startHeight.HasValue || _height.HasValue || _width.HasValue)
                    {
                        float num = _width.GetValueOrDefault(obstacleData.width) * singleLineWidth;
                        Vector3 b = new Vector3((num - singleLineWidth) * 0.5f, _startHeight.GetValueOrDefault(0), 0);
                        ____startPos = startPos + b;
                        ____midPos = midPos + b;
                        ____endPos = endPos + b;

                        float length = (____endPos - ____midPos).magnitude / move2Duration * obstacleData.duration;
                        float multiplier = _height.GetValueOrDefault(1); // No multiplier if _height no exist
                        ____stretchableObstacle.SetSizeAndColor(num * 0.98f, height * multiplier, length, ____color.color);
                        ____bounds = ____stretchableObstacle.bounds;
                    }
                }
            }
        }
    }
}