using CustomJSONData;
using HarmonyLib;
using NoodleExtensions.Animation;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;

namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(NoteJump))]
    [NoodlePatch("ManualUpdate")]
    internal class NoteJumpManualUpdate
    {
        private static readonly FieldInfo _localPositionField = AccessTools.Field(typeof(NoteJump), "_localPosition");
        private static readonly MethodInfo _definiteNoteJump = SymbolExtensions.GetMethodInfo(() => DefiniteNoteJump(Vector3.zero, 0));
        private static readonly MethodInfo _convertToLocalSpace = SymbolExtensions.GetMethodInfo(() => ConvertToLocalSpace(null));
        private static readonly MethodInfo _convertQuaternion = SymbolExtensions.GetMethodInfo(() => ConvertQuaternion(Quaternion.identity));
        private static readonly FieldInfo _definitePositionField = AccessTools.Field(typeof(NoteJumpManualUpdate), "_definitePosition");

#pragma warning disable 0414
        private static bool _definitePosition = false;
#pragma warning restore 0414

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundPosition = false;
            bool foundTransformUp = false;
            bool foundZOffset = false;
            bool foundInverseRotation = false;
            int instructionListCount = instructionList.Count;
            for (int i = 0; i < instructionListCount; i++)
            {
                if (!foundPosition &&
                    instructionList[i].opcode == OpCodes.Stind_R4)
                {
                    foundPosition = true;
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 3, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 4, new CodeInstruction(OpCodes.Ldfld, _localPositionField));
                    instructionList.Insert(i + 5, new CodeInstruction(OpCodes.Ldloc_1));
                    instructionList.Insert(i + 6, new CodeInstruction(OpCodes.Call, _definiteNoteJump));
                    instructionList.Insert(i + 7, new CodeInstruction(OpCodes.Stfld, _localPositionField));
                }
                if (!foundTransformUp &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                  ((MethodInfo)instructionList[i].operand).Name == "get_up")
                {
                    foundTransformUp = true;
                    instructionList[i] = new CodeInstruction(OpCodes.Call, _convertToLocalSpace);
                }
                if (instructionList[i].opcode == OpCodes.Ldfld &&
                 ((FieldInfo)instructionList[i].operand).Name == "_inverseWorldRotation")
                {
                    if (!foundInverseRotation)
                    {
                        foundInverseRotation = true;
                        continue;
                    }
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, _convertQuaternion));
                }
                if (!foundZOffset &&
                    instructionList[i].operand is Label &&
                    instructionList[i].operand.GetHashCode() == 21) // is there a better way of checking labels?
                {
                    foundZOffset = true;
                    // Add addition check to our quirky little variable to skip end position offset when we are using definitePosition
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Ldsfld, _definitePositionField));
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Brtrue_S, instructionList[i].operand));
                }
            }
            if (!foundPosition) Logger.Log("Failed to find stind.r4!", IPA.Logging.Logger.Level.Error);
            if (!foundTransformUp) Logger.Log("Failed to find call to get_up!", IPA.Logging.Logger.Level.Error);
            if (!foundZOffset) Logger.Log("Failed to find brfalse.s to Label21!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
        }

        private static Vector3 DefiniteNoteJump(Vector3 original, float time)
        {
            dynamic dynData = NoteControllerUpdate._customNoteData.customData;
            dynamic animationObject = Trees.at(dynData, "_animation");
            Track track = Trees.at(dynData, "track");
            AnimationHelper.GetDefinitePositionOffset(animationObject, track, time, out Vector3? position);
            if (position.HasValue)
            {
                Vector3 noteOffset = Trees.at(dynData, "noteOffset");
                _definitePosition = true;
                return position.Value + noteOffset;
            }
            else
            {
                _definitePosition = false;
                return original;
            }
        }

        // These methods are necessary in order to rotate the parent transform without screwing with the rotateObject's up
        // (This is something that beat games should be doing tbh)
        private static Vector3 ConvertToLocalSpace(Transform rotatedObject)
        {
            return rotatedObject.parent.InverseTransformDirection(rotatedObject.up);
        }

        private static Quaternion ConvertQuaternion(Quaternion source)
        {
            Vector3 euler = source.eulerAngles;
            euler.z = 0;
            return Quaternion.Euler(euler);
        }
    }
}
