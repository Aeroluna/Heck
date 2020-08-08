namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using CustomJSONData;
    using HarmonyLib;
    using NoodleExtensions.Animation;
    using UnityEngine;

    [NoodlePatch(typeof(NoteJump))]
    [NoodlePatch("ManualUpdate")]
    internal static class NoteJumpManualUpdate
    {
        private static readonly FieldInfo _jumpDurationField = AccessTools.Field(typeof(NoteJump), "_jumpDuration");
        private static readonly MethodInfo _noteJumpTimeAdjust = SymbolExtensions.GetMethodInfo(() => NoteJumpTimeAdjust(0, 0));
        private static readonly FieldInfo _localPositionField = AccessTools.Field(typeof(NoteJump), "_localPosition");
        private static readonly MethodInfo _definiteNoteJump = SymbolExtensions.GetMethodInfo(() => DefiniteNoteJump(Vector3.zero, 0));
        private static readonly MethodInfo _convertToLocalSpace = SymbolExtensions.GetMethodInfo(() => ConvertToLocalSpace(null));
        private static readonly MethodInfo _popQuaternion = SymbolExtensions.GetMethodInfo(() => PopQuaternion(Quaternion.identity, Vector3.zero));
        private static readonly FieldInfo _definitePositionField = AccessTools.Field(typeof(NoteJumpManualUpdate), "_definitePosition");
        private static readonly MethodInfo _setLocalPosition = typeof(Transform).GetProperty("localPosition").GetSetMethod();

#pragma warning disable 0414
        private static bool _definitePosition = false;
#pragma warning restore 0414

        internal static float NoteJumpTimeAdjust(float original, float jumpDuration)
        {
            dynamic dynData = NoteControllerUpdate.CustomNoteData.customData;
            Track track = Trees.at(dynData, "track");
            float? time = AnimationHelper.TryGetProperty(track, NoodleExtensions.Plugin.TIME);
            if (time.HasValue)
            {
                return Mathf.Clamp(time.Value, 0, 1) * jumpDuration;
            }
            else
            {
                return original;
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundTime = false;
            bool foundFinalPosition = false;
            bool foundTransformUp = false;
            bool foundZOffset = false;
            bool foundInverseRotation = false;
            bool foundPosition = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundTime &&
                    instructionList[i].opcode == OpCodes.Stloc_0)
                {
                    foundTime = true;
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Ldfld, _jumpDurationField));
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Call, _noteJumpTimeAdjust));
                }

                if (!foundFinalPosition &&
                    instructionList[i].opcode == OpCodes.Stind_R4)
                {
                    foundFinalPosition = true;
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
                    instructionList[i + 1] = new CodeInstruction(OpCodes.Call, _popQuaternion);
                }

                // is there a better way of checking labels?
                if (!foundZOffset &&
                    instructionList[i].operand is Label &&
                    instructionList[i].operand.GetHashCode() == 21)
                {
                    foundZOffset = true;

                    // Add addition check to our quirky little variable to skip end position offset when we are using definitePosition
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Ldsfld, _definitePositionField));
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Brtrue_S, instructionList[i].operand));
                }

                if (!foundPosition &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "set_position")
                {
                    foundPosition = true;
                    instructionList[i].operand = _setLocalPosition;
                }
            }

            if (!foundTime)
            {
                NoodleLogger.Log("Failed to find stloc.0!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundFinalPosition)
            {
                NoodleLogger.Log("Failed to find stind.r4!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundTransformUp)
            {
                NoodleLogger.Log("Failed to find call to get_up!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundZOffset)
            {
                NoodleLogger.Log("Failed to find brfalse.s to Label21!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundPosition)
            {
                NoodleLogger.Log("Failed to find callvirt to set_position!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static Vector3 DefiniteNoteJump(Vector3 original, float time)
        {
            dynamic dynData = NoteControllerUpdate.CustomNoteData.customData;
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

        // used to pop the quaternion from the stack and return the vector3
        private static Vector3 PopQuaternion(Quaternion quaternion, Vector3 vector)
        {
            return vector;
        }
    }
}
