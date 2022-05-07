using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using UnityEngine;
using static Heck.HeckController;

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
            if (__instance is not CustomObstacleData customNoteData)
            {
                return;
            }

            CustomData customData = customNoteData.customData;
            IEnumerable<float?>? position = customData.GetNullableFloats(V2_POSITION)?.ToList();
            IEnumerable<float?>? scale = customData.GetNullableFloats(V2_SCALE)?.ToList();
            Vector3? localrot = customData.GetVector3(V2_LOCAL_ROTATION);
            object? rotation = customData.Get<object>(V2_ROTATION);

            float? startX = position?.ElementAtOrDefault(0);
            float? scaleX = scale?.ElementAtOrDefault(0);

            float width = scaleX.GetValueOrDefault(__instance.width);
            if (startX.HasValue)
            {
                customData[V2_POSITION] = new List<object?> { (startX.Value + width) * -1, position!.ElementAtOrDefault(1) };
            }
            else if (scaleX.HasValue)
            {
                float lineIndex = __instance.lineIndex - 2;
                customData[V2_POSITION] = new List<object?> { (lineIndex + width) * -1, position?.ElementAtOrDefault(1) ?? 0 };
            }

            if (localrot != null)
            {
                Quaternion modifiedVector = Quaternion.Euler(localrot.Value);
                Vector3 vector = new Quaternion(modifiedVector.x, modifiedVector.y * -1, modifiedVector.z * -1, modifiedVector.w).eulerAngles;
                customData[V2_LOCAL_ROTATION] = new List<object> { vector.x, vector.y, vector.z };
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
                    customData[V2_ROTATION] = new List<object> { vector.x, vector.y, vector.z };
                }
                else
                {
                    customData[V2_ROTATION] = Convert.ToSingle(rotation) * -1;
                }
            }
        }
    }
}
