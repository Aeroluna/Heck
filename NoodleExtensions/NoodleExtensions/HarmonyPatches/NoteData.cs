using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteData))]
    [HarmonyPatch("MirrorLineIndex")]
    internal class NoteDataMirrorLineIndex
    {
        private static void Postfix(NoteData __instance)
        {
            if (__instance is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float?> _position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                float? flipLineIndex = (float?)Trees.at(dynData, "flipLineIndex");
                float? _rotation = (float?)Trees.at(dynData, ROTATION);

                float? _startRow = _position?.ElementAtOrDefault(0);

                IDictionary<string, object> dictdata = dynData as IDictionary<string, object>;

                if (_startRow.HasValue) dictdata[POSITION] = new List<object>() { ((_startRow.Value + 0.5f) * -1) - 0.5f, _position.ElementAtOrDefault(1) };
                if (flipLineIndex.HasValue) dynData.flipLineIndex = ((flipLineIndex.Value + 0.5f) * -1) - 0.5f;
                if (_rotation.HasValue) dictdata[ROTATION] = _rotation * -1;
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
                float? _rotation = (float?)Trees.at(dynData, CUTDIRECTION);

                if (_rotation.HasValue) dynData._rotation = 360 - _rotation.Value;
            }
        }
    }
}