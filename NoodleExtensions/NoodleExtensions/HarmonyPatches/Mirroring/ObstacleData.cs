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
                IEnumerable<float?> position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                IEnumerable<float?> scale = ((List<object>)Trees.at(dynData, SCALE))?.Select(n => n.ToNullableFloat());
                List<float> localrot = ((List<object>)Trees.at(dynData, LOCALROTATION))?.Select(n => Convert.ToSingle(n)).ToList();
                float? rotation = (float?)Trees.at(dynData, ROTATION);

                float? startX = position?.ElementAtOrDefault(0);
                float? scaleX = scale?.ElementAtOrDefault(0);

                IDictionary<string, object> dictdata = dynData as IDictionary<string, object>;

                float width = scaleX.GetValueOrDefault(__instance.width);
                if (startX.HasValue) dictdata[POSITION] = new List<object>() { (startX.Value + width) * -1, position.ElementAtOrDefault(1) };
                if (localrot != null) dictdata[LOCALROTATION] = localrot.Select(n => n *= 1).Cast<object>().ToList();
                if (rotation.HasValue) dictdata[ROTATION] = rotation * -1;
            }
        }
    }
}