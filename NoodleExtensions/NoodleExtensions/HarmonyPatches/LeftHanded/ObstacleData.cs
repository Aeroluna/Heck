namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using Heck;
    using UnityEngine;
    using static NoodleExtensions.Plugin;

    [HarmonyPatch(typeof(ObstacleData))]
    [HarmonyPatch("Mirror")]
    internal static class ObstacleDataMirror
    {
        private static void Prefix(ObstacleData __instance) // prefix because we need to know the lineIndex before it gets mirrored
        {
            if (__instance is CustomObstacleData customData)
            {
                Dictionary<string, object?> dynData = customData.customData;
                IEnumerable<float?>? position = dynData.GetNullableFloats(POSITION);
                IEnumerable<float?>? scale = dynData.GetNullableFloats(SCALE);
                List<float>? localrot = dynData.Get<List<object>>(LOCALROTATION)?.Select(n => Convert.ToSingle(n)).ToList();
                object? rotation = dynData.Get<object>(ROTATION);

                float? startX = position?.ElementAtOrDefault(0);
                float? scaleX = scale?.ElementAtOrDefault(0);

                float width = scaleX.GetValueOrDefault(__instance.width);
                if (startX.HasValue)
                {
                    dynData[POSITION] = new List<object?>() { (startX.Value + width) * -1, position.ElementAtOrDefault(1) };
                }
                else if (scaleX.HasValue)
                {
                    float lineIndex = __instance.lineIndex - 2;
                    dynData[POSITION] = new List<object?>() { (lineIndex + width) * -1, position?.ElementAtOrDefault(1) ?? 0 };
                }

                if (localrot != null)
                {
                    List<float> rot = localrot.Select(n => Convert.ToSingle(n)).ToList();
                    Quaternion modifiedVector = Quaternion.Euler(rot[0], rot[1], rot[2]);
                    Vector3 vector = new Quaternion(modifiedVector.x, modifiedVector.y * -1, modifiedVector.z * -1, modifiedVector.w).eulerAngles;
                    dynData[LOCALROTATION] = new List<object> { vector.x, vector.y, vector.z };
                }

                if (rotation != null)
                {
                    if (rotation is List<object> list)
                    {
                        List<float> rot = list.Select(n => Convert.ToSingle(n)).ToList();
                        Quaternion modifiedVector = Quaternion.Euler(rot[0], rot[1], rot[2]);
                        Vector3 vector = new Quaternion(modifiedVector.x, modifiedVector.y * -1, modifiedVector.z * -1, modifiedVector.w).eulerAngles;
                        dynData[ROTATION] = new List<object> { vector.x, vector.y, vector.z };
                    }
                    else
                    {
                        dynData[ROTATION] = Convert.ToSingle(rotation) * -1;
                    }
                }
            }
        }
    }
}
