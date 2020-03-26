using BS_Utils.Utilities;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteController))]
    [HarmonyPatch("Init")]
    internal class NoteControllerInit
    {
        private static void Prefix(ref NoteController __instance, NoteData noteData, ref Vector3 moveStartPos, ref Vector3 moveEndPos, ref Vector3 jumpEndPos,
            ref float jumpGravity, ref float worldRotation, ref float? __state)
        {
            if (NoodleExtensionsActive && !MappingExtensionsActive && noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                IEnumerable<float?> _position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                float? _rotation = (float?)Trees.at(dynData, ROTATION);

                float? _startRow = _position?.ElementAtOrDefault(0);
                float? _startHeight = _position?.ElementAtOrDefault(1);

                float _globalJumpOffsetY = beatmapObjectSpawnMovementData.GetPrivateField<float>("_globalJumpOffsetY");
                float _moveDistance = beatmapObjectSpawnMovementData.GetPrivateField<float>("_moveDistance");
                float _jumpDistance = beatmapObjectSpawnMovementData.GetPrivateField<float>("_jumpDistance");
                float _noteJumpMovementSpeed = beatmapObjectSpawnMovementData.GetPrivateField<float>("_noteJumpMovementSpeed");

                Vector3 forward2 = beatmapObjectSpawnController.transform.forward;
                Vector3 a4 = beatmapObjectSpawnController.transform.position;
                a4 += forward2 * (_moveDistance + _jumpDistance * 0.5f);
                Vector3 a5 = a4 - forward2 * _moveDistance;
                Vector3 a6 = a4 - forward2 * (_moveDistance + _jumpDistance);

                Vector3 noteOffset2 = GetNoteOffset(noteData, _startRow, _startHeight);

                if (noteData.noteType == NoteType.Bomb)
                {
                    __instance.transform.SetPositionAndRotation(a4 + noteOffset2, Quaternion.identity);
                    moveStartPos = a4 + noteOffset2;
                    moveEndPos = a5 + noteOffset2;
                    jumpEndPos = a6 + noteOffset2;
                }
                else if (noteData.noteType.IsBasicNote())
                {
                    float? flipLineIndex = (float?)Trees.at(dynData, "flipLineIndex");
                    Vector3 noteOffset3 = GetNoteOffset(noteData, flipLineIndex ?? _startRow, _startHeight);
                    __instance.transform.SetPositionAndRotation(a4 + noteOffset3, Quaternion.identity);
                    moveStartPos = a4 + noteOffset3;
                    moveEndPos = a5 + noteOffset3;
                    jumpEndPos = a6 + noteOffset2;
                }

                float lineYPos = LineYPosForLineLayer(noteData, _startHeight);
                // Magic numbers below found with linear regression y=mx+b using existing HighestJumpPosYForLineLayer values
                float highestJump = _startHeight.HasValue ? ((0.875f * lineYPos) + 0.639583f) + _globalJumpOffsetY :
                    beatmapObjectSpawnMovementData.HighestJumpPosYForLineLayer(noteData.noteLineLayer);
                jumpGravity = 2f * (highestJump - lineYPos) /
                    Mathf.Pow(_jumpDistance / _noteJumpMovementSpeed * 0.5f, 2f);

                // Precision 360 on individual note
                if (_rotation.HasValue) worldRotation = _rotation.Value;

                // flipYSide stuff
                float? flipYSide = (float?)Trees.at(dynData, "flipYSide");
                if (flipYSide.HasValue)
                {
                    __state = customData.flipYSide;
                    customData.SetProperty("flipYSide", flipYSide.Value, typeof(NoteData));
                }
            }
        }

        private static void Postfix(NoteController __instance, NoteData noteData, float? __state)
        {
            if (NoodleExtensionsActive && !MappingExtensionsActive && noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                float? _rot = (float?)Trees.at(dynData, CUTDIRECTION);
                if (!_rot.HasValue) return;

                NoteMovement noteMovement = __instance.GetPrivateField<NoteMovement>("_noteMovement");
                NoteJump noteJump = noteMovement.GetPrivateField<NoteJump>("_jump");

                Quaternion rotation = Quaternion.Euler(0, 0, _rot.Value);
                noteJump.SetPrivateField("_endRotation", rotation);
                Vector3 vector = rotation.eulerAngles;
                vector += noteJump.GetPrivateField<Vector3[]>("_randomRotations")[noteJump.GetPrivateField<int>("_randomRotationIdx")] * 20;
                Quaternion midrotation = Quaternion.Euler(vector);
                noteJump.SetPrivateField("_middleRotation", midrotation);

                // Reset flipYSide after Prefix
                if (__state.HasValue) customData.SetProperty("flipYSide", __state.Value, typeof(NoteData));
            }
        }
    }
}