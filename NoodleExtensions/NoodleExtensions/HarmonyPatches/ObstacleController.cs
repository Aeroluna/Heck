using BS_Utils.Utilities;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using System.Linq;
using UnityEngine;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPriority(Priority.Normal)]
    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("Init")]
    internal class ObstacleControllerInit
    {
        public static void Postfix(ref ObstacleController __instance, ObstacleData obstacleData, Vector3 startPos, Vector3 midPos, Vector3 endPos, float move2Duration,
            float singleLineWidth, ref Vector3 ____startPos, ref Vector3 ____endPos, ref Vector3 ____midPos,
            ref StretchableObstacle ____stretchableObstacle, ref Bounds ____bounds, SimpleColorSO ____color, float height)
        {
            // CustomJSONData
            if (NoodleExtensionsActive && !MappingExtensionsActive && obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                float? _startRow = (float?)Trees.at(dynData, "_startRow");
                float? _startHeight = (float?)Trees.at(dynData, "_startHeight");
                float? _height = (float?)Trees.at(dynData, "_height");
                float? _width = (float?)Trees.at(dynData, "_width");
                Vector3? _rot = Trees.getVector3(dynData, "_rotation");

                // Actual wall stuff
                if (_startRow.HasValue || _startHeight.HasValue || _width.HasValue || _height.HasValue)
                {
                    if (_startRow.HasValue || _startHeight.HasValue)
                    {
                        float _topObstaclePosY = beatmapObjectSpawnController.GetField<float>("_topObstaclePosY");
                        float _globalJumpOffsetY = beatmapObjectSpawnController.GetField<float>("_globalJumpOffsetY");
                        float _verticalObstaclePosY = beatmapObjectSpawnController.GetField<float>("_verticalObstaclePosY");
                        float _moveDistance = beatmapObjectSpawnController.GetField<float>("_moveDistance");
                        float _jumpDistance = beatmapObjectSpawnController.GetField<float>("_jumpDistance");

                        Vector3 forward = beatmapObjectSpawnController.transform.forward;
                        Vector3 a = beatmapObjectSpawnController.transform.position;
                        a += forward * (_moveDistance + _jumpDistance * 0.5f);
                        Vector3 a2 = a - forward * _moveDistance;
                        Vector3 a3 = a - forward * (_moveDistance + _jumpDistance);

                        // Ripped from base game
                        Vector3 noteOffset = GetNoteOffset(obstacleData, _startRow, null);
                        noteOffset.y = _startHeight.HasValue ? _verticalObstaclePosY : ((obstacleData.obstacleType == ObstacleType.Top)
                            ? (_topObstaclePosY + _globalJumpOffsetY) : _verticalObstaclePosY); ; // If _startHeight is set, put wall on floor
                        startPos = a + noteOffset;
                        midPos = a2 + noteOffset;
                        endPos = a3 + noteOffset;
                    }

                    // oh my god im actually adding rotation
                    if (_rot.HasValue) __instance.transform.Rotate(_rot.Value);

                    // Below ripped from base game
                    float num = _width.GetValueOrDefault(obstacleData.width) * singleLineWidth;
                    Vector3 b = new Vector3((num - singleLineWidth) * 0.5f, _startHeight.GetValueOrDefault(0), 0); // We add _startHeight here
                    ____startPos = startPos + b;
                    ____midPos = midPos + b;
                    ____endPos = endPos + b;

                    float length = (____endPos - ____midPos).magnitude / move2Duration * obstacleData.duration;
                    float trueHeight = _height.GetValueOrDefault(height) * (_height.HasValue ? singleLineWidth : 1); // Take _type as height if _height no exist
                    ____stretchableObstacle.SetSizeAndColor(num * 0.98f, trueHeight, length, ____color.color);
                    ____bounds = ____stretchableObstacle.bounds;

                    Logger.Log("_startRow:" + _startRow?.ToString() ?? "Null");
                    Logger.Log("_startHeight:" + _startHeight?.ToString() ?? "Null");
                    Logger.Log("_width:" + _width?.ToString() ?? "Null");
                    Logger.Log("_height:" + _height?.ToString() ?? "Null");
                }
            }
        }
    }
}