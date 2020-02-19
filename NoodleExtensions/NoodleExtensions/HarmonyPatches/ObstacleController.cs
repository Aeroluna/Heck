using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using BS_Utils.Utilities;
using Harmony;
using System.Linq;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPriority(Priority.Normal)]
    [HarmonyPatch(typeof(ObstacleController))]
    [HarmonyPatch("Init")]
    internal class ObstacleControllerInit
    {
        private static BeatmapObjectSpawnController beatmapObjectSpawnController;
        public static void Postfix(ref ObstacleController __instance, ObstacleData obstacleData, Vector3 startPos, Vector3 midPos, Vector3 endPos, float move2Duration,
            float singleLineWidth, ref Vector3 ____startPos, ref Vector3 ____endPos, ref Vector3 ____midPos,
            ref StretchableObstacle ____stretchableObstacle, ref Bounds ____bounds, SimpleColorSO ____color, float height)
        {
            if (obstacleData is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                if (dynData != null)
                {
                    float? _startRow = (float?)Trees.at(dynData, "_startRow");
                    float? _startHeight = (float?)Trees.at(dynData, "_startHeight");
                    float? _height = (float?)Trees.at(dynData, "_height");
                    float? _width = (float?)Trees.at(dynData, "_width");
                    float? _rotX = (float?)Trees.at(dynData, "_rotationX");
                    float? _rotY = (float?)Trees.at(dynData, "_rotationY");
                    float? _rotZ = (float?)Trees.at(dynData, "_rotationZ");

                    if (_startRow.HasValue || _startHeight.HasValue || _height.HasValue || _width.HasValue || _rotX.HasValue || _rotY.HasValue || _rotZ.HasValue)
                    {
                        // WARNING: THE GUY WHO WROTE THIS IS PEPEGA!
                        if (_startRow.HasValue || _startHeight.HasValue)
                        {
                            if (beatmapObjectSpawnController == null) beatmapObjectSpawnController = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First();

                            float _topObstaclePosY = beatmapObjectSpawnController.GetField<float>("_topObstaclePosY");
                            float _globalJumpOffsetY = beatmapObjectSpawnController.GetField<float>("_globalJumpOffsetY");
                            float _verticalObstaclePosY = beatmapObjectSpawnController.GetField<float>("_verticalObstaclePosY");
                            float _noteLinesCount = beatmapObjectSpawnController.GetField<float>("_noteLinesCount"); // This is always 4, but lets be safe
                            float _noteLinesDistance = beatmapObjectSpawnController.GetField<float>("_noteLinesDistance");

                            // This stuff below looks weird but basically it subtracts the offset from the obstacle's position so we can calculate that ourselves
                            Vector3 noteOffset = beatmapObjectSpawnController.GetNoteOffset(obstacleData.lineIndex, NoteLineLayer.Base);
                            noteOffset.y = obstacleData.obstacleType == ObstacleType.Top ? (_topObstaclePosY + _globalJumpOffsetY) : _verticalObstaclePosY;
                            startPos -= noteOffset;
                            midPos -= noteOffset;
                            endPos -= noteOffset;

                            // Ripped from base game
                            float distance = -(_noteLinesCount - 1) * 0.5f + (_noteLinesCount / 2); // Add last part to simulate https://github.com/spookyGh0st/beatwalls/#wall
                            distance = (distance + _startRow.GetValueOrDefault(obstacleData.lineIndex)) * _noteLinesDistance;
                            Vector3 trueOffset = beatmapObjectSpawnController.transform.right * distance
                                + new Vector3(0, beatmapObjectSpawnController.LineYPosForLineLayer(NoteLineLayer.Base), 0);
                            trueOffset.y = _startHeight.HasValue ? _verticalObstaclePosY : noteOffset.y; // If _startHeight is set, put wall on floor
                            startPos += trueOffset;
                            midPos += trueOffset;
                            endPos += trueOffset;
                        }

                        // oh my god im actually adding rotation
                        if (_rotX.HasValue || _rotY.HasValue || _rotZ.HasValue)
                            __instance.transform.localEulerAngles = new Vector3(_rotX.GetValueOrDefault(0), _rotY.GetValueOrDefault(0), _rotZ.GetValueOrDefault(0));

                        // Below ripped from base game
                        float num = _width.GetValueOrDefault(obstacleData.width) * singleLineWidth;
                        Vector3 b = new Vector3((num - singleLineWidth) * 0.5f, _startHeight.GetValueOrDefault(0), 0); // We add _startHeight here
                        ____startPos = startPos + b;
                        ____midPos = midPos + b;
                        ____endPos = endPos + b;

                        float length = (____endPos - ____midPos).magnitude / move2Duration * obstacleData.duration;
                        float trueHeight = _height.GetValueOrDefault(height); // Take _type as height if _height no exist
                        ____stretchableObstacle.SetSizeAndColor(num * 0.98f, trueHeight, length, ____color.color);
                        ____bounds = ____stretchableObstacle.bounds;
                    }
                }
            }
        }
    }
}