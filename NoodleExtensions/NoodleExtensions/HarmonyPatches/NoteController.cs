using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
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
        private static void Postfix(NoteData noteData, NoteMovement ____noteMovement)
        {
            if (noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                float? _cutDir = (float?)Trees.at(dynData, CUTDIRECTION);

                NoteJump noteJump = ____noteMovement.GetField<NoteJump, NoteMovement>("_jump");
                NoteFloorMovement floorMovement = ____noteMovement.GetField<NoteFloorMovement, NoteMovement>("_floorMovement");

                if (_cutDir.HasValue)
                {
                    Quaternion rotation = Quaternion.Euler(0, 0, _cutDir.Value);
                    noteJump.SetField("_endRotation", rotation);
                    Vector3 vector = rotation.eulerAngles;
                    vector += noteJump.GetField<Vector3[], NoteJump>("_randomRotations")[noteJump.GetField<int, NoteJump>("_randomRotationIdx")] * 20;
                    Quaternion midrotation = Quaternion.Euler(vector);
                    noteJump.SetField("_middleRotation", midrotation);
                }

                dynamic _rotation = Trees.at(dynData, ROTATION);
                if (_rotation != null)
                {
                    Quaternion _worldRotation;
                    if (_rotation is List<object> list)
                    {
                        IEnumerable<float> _rot = (list)?.Select(n => Convert.ToSingle(n));
                        _worldRotation = Quaternion.Euler(_rot.ElementAt(0), _rot.ElementAt(1), _rot.ElementAt(2));
                    }
                    else _worldRotation = Quaternion.Euler(0, (float)_rotation, 0);
                    Quaternion _inverseWorldRotation = Quaternion.Inverse(_worldRotation);
                    noteJump.SetField("_worldRotation", _worldRotation);
                    noteJump.SetField("_inverseWorldRotation", _inverseWorldRotation);
                    floorMovement.SetField("_worldRotation", _worldRotation);
                    floorMovement.SetField("_inverseWorldRotation", _inverseWorldRotation);
                    floorMovement.SetToStart();
                }
            }
        }

        private static readonly MethodInfo getFlipYSide = SymbolExtensions.GetMethodInfo(() => GetFlipYSide(null, 0));
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundFlipYSide = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundFlipYSide &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "get_flipYSide")
                {
                    foundFlipYSide = true;

                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, getFlipYSide));
                    instructionList.Insert(i - 2, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i - 1, new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(NoteController), "_noteData")));
                }
            }
            if (!foundFlipYSide) Logger.Log("Failed to find Get_flipYSide call, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
        }

        private static float GetFlipYSide(NoteData noteData, float @default)
        {
            float _flipYSide = @default;
            if (noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;

                float? flipYSide = (float?)Trees.at(dynData, "flipYSide");
                if (flipYSide.HasValue) _flipYSide = flipYSide.Value;
            }
            return _flipYSide;
        }
    }
}