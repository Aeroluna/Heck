using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using UnityEngine;
using static Heck.HeckController;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.LeftHanded
{
    // TODO: rework left handed
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(NoteData))]
    internal static class MirrorNoteData
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(NoteData.Mirror))]
        private static void Postfix(NoteData __instance)
        {
            if (__instance is not CustomNoteData customNoteData)
            {
                return;
            }

            CustomData customData = customNoteData.customData;
            List<float?>? position = customData.GetNullableFloats(V2_POSITION)?.ToList();
            float? flipLineIndex = customData.Get<float?>("flipLineIndex");
            List<float?>? flip = customData.GetNullableFloats(V2_FLIP)?.ToList();
            Vector3? localrot = customData.GetVector3(V2_LOCAL_ROTATION);
            object? rotation = customData.Get<object>(V2_ROTATION);

            float? startRow = position?.ElementAtOrDefault(0);
            float? flipX = flip?.ElementAtOrDefault(0);

            static float MirrorIndex(float input)
            {
                return ((input + 0.5f) * -1) - 0.5f;
            }

            if (startRow.HasValue)
            {
                customData[V2_POSITION] = new List<object?> { MirrorIndex(startRow.Value), position!.ElementAtOrDefault(1) };
            }

            if (flipLineIndex.HasValue)
            {
                customData["flipLineIndex"] = MirrorIndex(flipLineIndex.Value);
            }

            if (flipX.HasValue)
            {
                customData[V2_FLIP] = new List<object?> { MirrorIndex(flipX.Value), flip!.ElementAtOrDefault(1) };
            }

            if (localrot != null)
            {
                Quaternion modifiedVector = Quaternion.Euler(localrot.Value);
                Vector3 vector = new Quaternion(modifiedVector.x, modifiedVector.y * -1, modifiedVector.z * -1, modifiedVector.w).eulerAngles;
                customData[V2_LOCAL_ROTATION] = new List<object> { vector.x, vector.y, vector.z };
            }

            if (rotation != null)
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

            float? cutDirection = customData.Get<float?>(V2_CUT_DIRECTION);

            if (cutDirection.HasValue)
            {
                customData[V2_CUT_DIRECTION] = 360 - cutDirection.Value;
            }
        }
    }
}
