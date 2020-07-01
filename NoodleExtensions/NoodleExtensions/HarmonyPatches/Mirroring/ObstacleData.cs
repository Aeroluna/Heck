namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using UnityEngine;
    using static NoodleExtensions.Plugin;

    [HarmonyPatch(typeof(ObstacleData))]
    [HarmonyPatch("MirrorLineIndex")]
    internal static class ObstacleDataMirrorLineIndex
    {
#pragma warning disable SA1313
        private static void Postfix(ObstacleData __instance)
#pragma warning restore SA1313
        {
            if (__instance is CustomObstacleData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float?> position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                IEnumerable<float?> scale = ((List<object>)Trees.at(dynData, SCALE))?.Select(n => n.ToNullableFloat());
                List<float> localrot = ((List<object>)Trees.at(dynData, LOCALROTATION))?.Select(n => Convert.ToSingle(n)).ToList();
                dynamic rotation = Trees.at(dynData, ROTATION);

                float? startX = position?.ElementAtOrDefault(0);
                float? scaleX = scale?.ElementAtOrDefault(0);

                IDictionary<string, object> dictdata = dynData as IDictionary<string, object>;

                float width = scaleX.GetValueOrDefault(__instance.width);
                if (startX.HasValue)
                {
                    dictdata[POSITION] = new List<object>() { (startX.Value + width) * -1, position.ElementAtOrDefault(1) };
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
}
