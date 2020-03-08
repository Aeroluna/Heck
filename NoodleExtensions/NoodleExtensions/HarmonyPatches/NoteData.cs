using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using System.Collections.Generic;
using System.Linq;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPriority(Priority.Normal)]
    [HarmonyPatch(typeof(NoteData))]
    [HarmonyPatch("MirrorLineIndex")]
    internal class NoteDataMirrorLineIndex
    {
        private static void Postfix(NoteData __instance)
        {
            if (__instance is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                float?[] _position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat()).ToArray();
                float? flipLineIndex = (float?)Trees.at(dynData, "flipLineIndex");
                float? _rotation = (float?)Trees.at(dynData, ROTATION);

                float? _startRow = _position?.ElementAtOrDefault(0);

                if (_startRow.HasValue) dynData._startRow = ((_startRow.Value + 0.5f) * -1) - 0.5f;
                if (flipLineIndex.HasValue) dynData.flipLineIndex = ((flipLineIndex.Value + 0.5f) * -1) - 0.5f;
                if (_rotation.HasValue) dynData._rotation = _rotation * -1;
            }
        }
    }

    [HarmonyPriority(Priority.Normal)]
    [HarmonyPatch(typeof(NoteData))]
    [HarmonyPatch("MirrorTransformCutDirection")]
    internal class NoteDataMirrorTransformCutDirection
    {
        private static void Postfix(NoteData __instance)
        {
            if (__instance is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                float? _rotation = (float?)Trees.at(dynData, CUTDIRECTION);

                if (_rotation.HasValue) dynData._rotation = 360 - _rotation.Value;
            }
        }
    }
}