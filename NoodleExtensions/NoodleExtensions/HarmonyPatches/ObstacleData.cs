using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using System.Collections.Generic;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPriority(Priority.Normal)]
    [HarmonyPatch(typeof(ObstacleData))]
    [HarmonyPatch("MirrorLineIndex")]
    internal class ObstacleDataMirrorLineIndex
    {
        public static void Postfix(ObstacleData __instance)
        {
            if (__instance is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                float? _startRow = (float?)Trees.at(dynData, "_startRow");
                float? _width = (float?)Trees.at(dynData, "_width");
                Vector3? _localrot = Trees.getVector3(dynData, "_localRotation");
                float? _rotation = Trees.at(dynData, "_rotation");

                float width = _width.GetValueOrDefault(__instance.width);
                if (_startRow.HasValue) dynData._startRow = (_startRow.Value + width) * -1;

                if (_localrot.HasValue)
                {
                    _localrot *= -1;
                    List<object> rotation = new List<object>() { _localrot.Value.x, _localrot.Value.y, _localrot.Value.z };
                    dynData._rotation = rotation;
                }

                if (_rotation.HasValue) dynData._rotation = _rotation * -1;
            }
        }
    }
}