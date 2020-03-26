using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NoodleExtensions.NoodleController;
using static NoodleExtensions.NoodleController.BeatmapObjectSpawnMovementDataVariables;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(NoteController))]
    [NoodlePatch("Init")]
    internal class NoteControllerInit
    {
        private static void Prefix(NoteData noteData, ref Vector3 moveStartPos, ref Vector3 moveEndPos, ref Vector3 jumpEndPos,
            ref float jumpGravity, ref float worldRotation, ref float? __state)
        {
            if (noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float?> _position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                float? _rotation = (float?)Trees.at(dynData, ROTATION);

                float? _startRow = _position?.ElementAtOrDefault(0);
                float? _startHeight = _position?.ElementAtOrDefault(1);

                Vector3 noteOffset = GetNoteOffset(noteData, _startRow, _startHeight);

                jumpEndPos = _jumpEndPos + noteOffset;
                if (noteData.noteType.IsBasicNote())
                {
                    float? flipLineIndex = (float?)Trees.at(dynData, "flipLineIndex");
                    Vector3 noteOffset2 = GetNoteOffset(noteData, flipLineIndex ?? _startRow, _startHeight);
                    moveStartPos = _moveStartPos + noteOffset2;
                    moveEndPos = _moveEndPos + noteOffset2;
                }
                else
                {
                    moveStartPos = _moveStartPos + noteOffset;
                    moveEndPos = _moveEndPos + noteOffset;
                }

                float lineYPos = LineYPosForLineLayer(noteData, _startHeight);
                // Magic numbers below found with linear regression y=mx+b using existing HighestJumpPosYForLineLayer values
                float highestJump = _startHeight.HasValue ? ((0.875f * lineYPos) + 0.639583f) + _jumpOffsetY :
                    beatmapObjectSpawnMovementData.HighestJumpPosYForLineLayer(noteData.noteLineLayer);
                jumpGravity = 2f * (highestJump - lineYPos) /
                    Mathf.Pow(_jumpDistance / _noteJumpMovementSpeed * 0.5f, 2f);

                // Precision 360 on individual note
                if (_rotation.HasValue) worldRotation = _rotation.Value;

                // flipYSide stuff
                float? flipYSide = (float?)Trees.at(dynData, "flipYSide");
                if (flipYSide.HasValue)
                {
                    __state = noteData.flipYSide;
                    typeof(NoteData).GetProperty("flipYSide").SetValue(noteData, __state.Value);
                }
            }
        }

        private static void Postfix(NoteController __instance, NoteData noteData, float? __state)
        {
            if (noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                float? _rot = (float?)Trees.at(dynData, CUTDIRECTION);
                if (!_rot.HasValue) return;

                Traverse noteJump = new Traverse(Traverse.Create(Traverse.Create(__instance).Field("_noteMovement").GetValue()).Field("_jump").GetValue());

                Quaternion rotation = Quaternion.Euler(0, 0, _rot.Value);
                noteJump.Field("_endRotation").SetValue(rotation);
                Vector3 vector = rotation.eulerAngles;
                vector += ((Vector3[])noteJump.Field("_randomRotations").GetValue())[(int)noteJump.Field("_randomRotationIdx").GetValue()] * 20;
                Quaternion midrotation = Quaternion.Euler(vector);
                noteJump.Field("_middleRotation").SetValue(midrotation);

                // Reset flipYSide after Prefix
                if (__state.HasValue) typeof(NoteData).GetProperty("flipYSide").SetValue(noteData, __state.Value);
            }
        }
    }
}