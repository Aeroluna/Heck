using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
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
                IEnumerable<float?> _position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                IEnumerable<float?> _scale = ((List<object>)Trees.at(dynData, SCALE))?.Select(n => n.ToNullableFloat());
                List<float> _localrot = ((List<object>)Trees.at(dynData, LOCALROTATION))?.Select(n => Convert.ToSingle(n)).ToList();
                float? _rotation = (float?)Trees.at(dynData, ROTATION);

                float? _startX = _position?.ElementAtOrDefault(0);
                float? _width = _scale?.ElementAtOrDefault(0);

                IDictionary<string, object> dictdata = dynData as IDictionary<string, object>;

                float width = _width.GetValueOrDefault(__instance.width);
                if (_startX.HasValue) dictdata[POSITION] = new List<object>() { (_startX.Value + width) * -1, _position.ElementAtOrDefault(1) };
                if (_localrot != null) dictdata[LOCALROTATION] = _localrot.Select(n => n *= 1).Cast<object>().ToList();
                if (_rotation.HasValue) dictdata[ROTATION] = _rotation * -1;
            }
        }
    }
}