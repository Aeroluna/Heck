using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using System.Collections.Generic;
using UnityEngine;
using static NoodleExtensions.Plugin;

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
                Vector3? _rot = Trees.getVector3(dynData, "_rotation");

                float width = _width.GetValueOrDefault(__instance.width);
                if (_startRow.HasValue) dynData._startRow = (_startRow.Value + width) * -1;

                if (_rot.HasValue)
                {
                    _rot *= -1;
                    List<object> rotation = new List<object>() {_rot.Value.x, _rot.Value.y, _rot.Value.z};
                    dynData._rotation = rotation;
                }
            }
        }
    }
}