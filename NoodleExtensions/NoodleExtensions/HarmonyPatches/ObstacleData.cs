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
                List<object> _position = Trees.at(dynData, POSITION);
                List<object> _scale = Trees.at(dynData, SCALE);
                Vector3? _localrot = Trees.getVector3(dynData, LOCALROTATION);
                float? _rotation = Trees.at(dynData, ROTATION);

                float? _startRow = (float?)_position[0];
                float? _width = (float?)_scale[0];

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