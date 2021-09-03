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

    [HarmonyPatch(typeof(NoteData))]
    [HarmonyPatch("Mirror")]
    internal static class NoteDataMirror
    {
        private static void Postfix(NoteData __instance)
        {
            if (__instance is CustomNoteData customData)
            {
                Dictionary<string, object?> dynData = customData.customData;
                IEnumerable<float?>? position = dynData.GetNullableFloats(POSITION);
                float? flipLineIndex = dynData.Get<float?>("flipLineIndex");
                IEnumerable<float?>? flip = dynData.GetNullableFloats(FLIP);
                List<float>? localrot = dynData.Get<List<object>>(LOCALROTATION)?.Select(n => Convert.ToSingle(n)).ToList();
                object? rotation = dynData.Get<object>(ROTATION);

                float? startRow = position?.ElementAtOrDefault(0);
                float? flipX = flip?.ElementAtOrDefault(0);

                if (startRow.HasValue)
                {
                    dynData[POSITION] = new List<object?>() { ((startRow.Value + 0.5f) * -1) - 0.5f, position.ElementAtOrDefault(1) };
                }

                if (flipLineIndex.HasValue)
                {
                    dynData["flipLineIndex"] = ((flipLineIndex.Value + 0.5f) * -1) - 0.5f;
                }

                if (flipX.HasValue)
                {
                    dynData[FLIP] = new List<object?>() { ((flipX.Value + 0.5f) * -1) - 0.5f, flip.ElementAtOrDefault(1) };
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

                float? cutDirection = dynData.Get<float?>(CUTDIRECTION);

                if (cutDirection.HasValue)
                {
                    dynData[CUTDIRECTION] = 360 - cutDirection.Value;
                }
            }
        }
    }
}
