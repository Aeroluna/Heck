using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using UnityEngine;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.LeftHanded
{
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(ObstacleData))]
    internal static class MirrorObstacleData
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ObstacleData.Mirror))]
        private static void Prefix(ObstacleData __instance) // prefix because we need to know the lineIndex before it gets mirrored
        {
            if (__instance is not CustomObstacleData customData)
            {
                return;
            }

            Dictionary<string, object?> dynData = customData.customData;
            IEnumerable<float?>? position = dynData.GetNullableFloats(POSITION)?.ToList();
            IEnumerable<float?>? scale = dynData.GetNullableFloats(SCALE)?.ToList();
            Vector3? localrot = dynData.GetVector3(LOCAL_ROTATION);
            object? rotation = dynData.Get<object>(ROTATION);

            float? startX = position?.ElementAtOrDefault(0);
            float? scaleX = scale?.ElementAtOrDefault(0);

            float width = scaleX.GetValueOrDefault(__instance.width);
            if (startX.HasValue)
            {
                dynData[POSITION] = new List<object?> { (startX.Value + width) * -1, position!.ElementAtOrDefault(1) };
            }
            else if (scaleX.HasValue)
            {
                float lineIndex = __instance.lineIndex - 2;
                dynData[POSITION] = new List<object?> { (lineIndex + width) * -1, position?.ElementAtOrDefault(1) ?? 0 };
            }

            if (localrot != null)
            {
                Quaternion modifiedVector = Quaternion.Euler(localrot.Value);
                Vector3 vector = new Quaternion(modifiedVector.x, modifiedVector.y * -1, modifiedVector.z * -1, modifiedVector.w).eulerAngles;
                dynData[LOCAL_ROTATION] = new List<object> { vector.x, vector.y, vector.z };
            }

            if (rotation == null)
            {
                return;
            }

            {
                if (rotation is List<object> list)
                {
                    List<float> rot = list.Select(Convert.ToSingle).ToList();
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
