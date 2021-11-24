using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using JetBrains.Annotations;
using UnityEngine;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.LeftHanded
{
    [HarmonyPatch(typeof(NoteData))]
    [HarmonyPatch("Mirror")]
    internal static class NoteDataMirror
    {
        [UsedImplicitly]
        private static void Postfix(NoteData __instance)
        {
            if (__instance is not CustomNoteData customData)
            {
                return;
            }

            Dictionary<string, object?> dynData = customData.customData;
            List<float?>? position = dynData.GetNullableFloats(POSITION)?.ToList();
            float? flipLineIndex = dynData.Get<float?>("flipLineIndex");
            List<float?>? flip = dynData.GetNullableFloats(FLIP)?.ToList();
            Vector3? localrot = dynData.GetVector3(LOCAL_ROTATION);
            object? rotation = dynData.Get<object>(ROTATION);

            float? startRow = position?.ElementAtOrDefault(0);
            float? flipX = flip?.ElementAtOrDefault(0);

            if (startRow.HasValue)
            {
                dynData[POSITION] = new List<object?> { ((startRow.Value + 0.5f) * -1) - 0.5f, position!.ElementAtOrDefault(1) };
            }

            if (flipLineIndex.HasValue)
            {
                dynData["flipLineIndex"] = ((flipLineIndex.Value + 0.5f) * -1) - 0.5f;
            }

            if (flipX.HasValue)
            {
                dynData[FLIP] = new List<object?> { ((flipX.Value + 0.5f) * -1) - 0.5f, flip!.ElementAtOrDefault(1) };
            }

            if (localrot != null)
            {
                Quaternion modifiedVector = Quaternion.Euler(localrot.Value);
                Vector3 vector = new Quaternion(modifiedVector.x, modifiedVector.y * -1, modifiedVector.z * -1, modifiedVector.w).eulerAngles;
                dynData[LOCAL_ROTATION] = new List<object> { vector.x, vector.y, vector.z };
            }

            if (rotation != null)
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

            float? cutDirection = dynData.Get<float?>(CUT_DIRECTION);

            if (cutDirection.HasValue)
            {
                dynData[CUT_DIRECTION] = 360 - cutDirection.Value;
            }
        }
    }
}
