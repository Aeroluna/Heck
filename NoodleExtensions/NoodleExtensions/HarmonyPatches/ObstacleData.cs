using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(ObstacleData))]
    [HarmonyPatch("MirrorLineIndex")]
    internal class ObstacleDataMirrorLineIndex
    {
        private static void Postfix(ObstacleData __instance)
        {
            if (__instance is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                float?[] _position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat()).ToArray();
                float?[] _scale = ((List<object>)Trees.at(dynData, SCALE))?.Select(n => n.ToNullableFloat()).ToArray();
                Vector3? _localrot = Trees.getVector3(dynData, LOCALROTATION);
                float? _rotation = Trees.at(dynData, ROTATION);

                float? _startX = _position?.ElementAtOrDefault(0);
                float? _width = _scale?.ElementAtOrDefault(0);

                float width = _width.GetValueOrDefault(__instance.width);
                if (_startX.HasValue) dynData._startX = (_startX.Value + width) * -1;

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