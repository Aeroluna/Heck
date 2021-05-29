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
    [HarmonyPatch("MirrorLineIndex")]
    internal static class NoteDataMirrorLineIndex
    {
        private static void Postfix(NoteData __instance)
        {
            if (__instance is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float?> position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                float? flipLineIndex = (float?)Trees.at(dynData, "flipLineIndex");
                IEnumerable<float?> flip = ((List<object>)Trees.at(dynData, FLIP))?.Select(n => n.ToNullableFloat());
                List<float> localrot = ((List<object>)Trees.at(dynData, LOCALROTATION))?.Select(n => Convert.ToSingle(n)).ToList();
                dynamic rotation = Trees.at(dynData, ROTATION);

                float? startRow = position?.ElementAtOrDefault(0);
                float? flipX = flip?.ElementAtOrDefault(0);

                IDictionary<string, object> dictdata = dynData as IDictionary<string, object>;

                if (startRow.HasValue)
                {
                    dictdata[POSITION] = new List<object>() { ((startRow.Value + 0.5f) * -1) - 0.5f, position.ElementAtOrDefault(1) };
                }

                if (flipLineIndex.HasValue)
                {
                    dynData.flipLineIndex = ((flipLineIndex.Value + 0.5f) * -1) - 0.5f;
                }

                if (flipX.HasValue)
                {
                    dictdata[FLIP] = new List<object>() { ((flipX.Value + 0.5f) * -1) - 0.5f, flip.ElementAtOrDefault(1) };
                }

                if (localrot != null)
                {
                    List<float> rot = localrot.Select(n => Convert.ToSingle(n)).ToList();
                    Quaternion modifiedVector = Quaternion.Euler(rot[0], rot[1], rot[2]);
                    Vector3 vector = new Quaternion(modifiedVector.x, modifiedVector.y * -1, modifiedVector.z * -1, modifiedVector.w).eulerAngles;
                    dictdata[LOCALROTATION] = new List<object> { vector.x, vector.y, vector.z };
                }

                if (rotation != null)
                {
                    if (rotation is List<object> list)
                    {
                        List<float> rot = list.Select(n => Convert.ToSingle(n)).ToList();
                        Quaternion modifiedVector = Quaternion.Euler(rot[0], rot[1], rot[2]);
                        Vector3 vector = new Quaternion(modifiedVector.x, modifiedVector.y * -1, modifiedVector.z * -1, modifiedVector.w).eulerAngles;
                        dictdata[ROTATION] = new List<object> { vector.x, vector.y, vector.z };
                    }
                    else
                    {
                        dictdata[ROTATION] = rotation * -1;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(NoteData))]
    [HarmonyPatch("MirrorTransformCutDirection")]
    internal class NoteDataMirrorTransformCutDirection
    {
        private static void Postfix(NoteData __instance)
        {
            if (__instance is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                float? rotation = (float?)Trees.at(dynData, CUTDIRECTION);

                IDictionary<string, object> dictdata = dynData as IDictionary<string, object>;

                if (rotation.HasValue)
                {
                    dictdata[CUTDIRECTION] = 360 - rotation.Value;
                }
            }
        }
    }
}
