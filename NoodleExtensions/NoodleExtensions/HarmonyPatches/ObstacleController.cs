using BS_Utils.Utilities;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("Init")]
    internal class ObstacleControllerInit
    {
        private static void Postfix(ref ObstacleController __instance, ObstacleData obstacleData, Vector3 startPos, Vector3 midPos, Vector3 endPos, float move2Duration,
            float singleLineWidth, ref Vector3 ____startPos, ref Vector3 ____endPos, ref Vector3 ____midPos,
            ref StretchableObstacle ____stretchableObstacle, ref Bounds ____bounds, SimpleColorSO ____color, float height, ref Quaternion ____worldRotation,
            ref Quaternion ____inverseWorldRotation)
        {
            if (NoodleExtensionsActive && !MappingExtensionsActive && obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float?> _position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                IEnumerable<float?> _scale = ((List<object>)Trees.at(dynData, SCALE))?.Select(n => n.ToNullableFloat());
                IEnumerable<float> _localrot = ((List<object>)Trees.at(dynData, LOCALROTATION))?.Select(n => Convert.ToSingle(n));
                float? _rotation = (float?)Trees.at(dynData, ROTATION);

                float? _startX = _position?.ElementAtOrDefault(0);
                float? _startY = _position?.ElementAtOrDefault(1);

                float? _width = _scale?.ElementAtOrDefault(0);
                float? _height = _scale?.ElementAtOrDefault(1);

                // Actual wall stuff
                if (_startX.HasValue || _startY.HasValue || _width.HasValue || _height.HasValue)
                {
                    if (_startX.HasValue || _startY.HasValue)
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
                        Vector3 noteOffset = GetNoteOffset(obstacleData, _startX, null);
                        noteOffset.y = _startY.HasValue ? _verticalObstaclePosY : ((obstacleData.obstacleType == ObstacleType.Top)
                            ? (_topObstaclePosY + _globalJumpOffsetY) : _verticalObstaclePosY); // If _startY(_startHeight) is set, put wall on floor
                        startPos = a + noteOffset;
                        midPos = a2 + noteOffset;
                        endPos = a3 + noteOffset;
                    }

                    // Below ripped from base game
                    float num = _width.GetValueOrDefault(obstacleData.width) * singleLineWidth;
                    Vector3 b = new Vector3((num - singleLineWidth) * 0.5f, _startY.GetValueOrDefault(0) * singleLineWidth, 0); // We add _startY(_startHeight) here
                    ____startPos = startPos + b;
                    ____midPos = midPos + b;
                    ____endPos = endPos + b;

                    float length = (____endPos - ____midPos).magnitude / move2Duration * obstacleData.duration;
                    float trueHeight = (_height * singleLineWidth) ?? height; // Take _type as height if _height no exist
                    ____stretchableObstacle.SetSizeAndColor(num * 0.98f, trueHeight, length, ____color.color);
                    ____bounds = ____stretchableObstacle.bounds;
                }

                // Precision 360 on individual wall
                if (_rotation.HasValue)
                {
                    ____worldRotation = Quaternion.Euler(0, _rotation.Value, 0);
                    ____inverseWorldRotation = Quaternion.Euler(0, -_rotation.Value, 0);
                    __instance.transform.localRotation = ____worldRotation;
                }

                // oh my god im actually adding rotation
                if (_localrot != null)
                {
                    Vector3 vector = new Vector3(_localrot.ElementAt(0), _localrot.ElementAt(1), _localrot.ElementAt(2));
                    __instance.transform.Rotate(vector);
                }
            }
        }
    }
}