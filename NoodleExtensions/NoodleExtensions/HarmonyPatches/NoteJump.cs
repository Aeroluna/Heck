namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
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
        private static readonly FieldInfo _definitePositionField = AccessTools.Field(typeof(NoteJumpManualUpdate), "_definitePosition");
        private static readonly MethodInfo _getTransform = typeof(Component).GetProperty("transform").GetGetMethod();
        private static readonly MethodInfo _doNoteLook = SymbolExtensions.GetMethodInfo(() => DoNoteLook(0, Quaternion.identity, Quaternion.identity, Quaternion.identity, null, Quaternion.identity, null, null));
        private static readonly FieldInfo _startRotationField = AccessTools.Field(typeof(NoteJump), "_startRotation");
        private static readonly FieldInfo _middleRotationField = AccessTools.Field(typeof(NoteJump), "_middleRotation");
        private static readonly FieldInfo _endRotationField = AccessTools.Field(typeof(NoteJump), "_endRotation");
        private static readonly FieldInfo _playerTransformsField = AccessTools.Field(typeof(NoteJump), "_playerTransforms");
        private static readonly FieldInfo _inverseWorldRotationField = AccessTools.Field(typeof(NoteJump), "_inverseWorldRotation");
        private static readonly FieldInfo _rotatedObjectField = AccessTools.Field(typeof(NoteJump), "_rotatedObject");

        // This field is used by reflection
#pragma warning disable CS0414 // The field is assigned but its value is never used
        private static bool _definitePosition = false;
#pragma warning restore CS0414 // The field is assigned but its value is never used

        internal static float NoteJumpTimeAdjust(float original, float jumpDuration)
        {
            NoodleObjectData noodleData = NoteControllerUpdate.NoodleData;
            if (noodleData != null)
            {
                float? time = (float?)AnimationHelper.TryGetPropertyAsObject(noodleData.Track, Plugin.TIME);
                if (time.HasValue)
                {
                    return time.Value * jumpDuration;
                }
            }

            return original;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundTime = false;
            bool foundFinalPosition = false;
            bool foundZOffset = false;
            bool foundLook = false;
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

                // Override all the rotation stuff
                if (!foundLook &&
                    instructionList[i].opcode == OpCodes.Bge_Un &&
                    instructionList[i].operand.GetHashCode() == 6)
                {
                    Label label = (Label)instructionList[i].operand;
                    int endIndex = instructionList.FindIndex(n => n.labels.Contains(label));

                    foundLook = true;

                    instructionList.RemoveRange(i + 1, endIndex - i - 1);

                    // This is where the fun begins
                    CodeInstruction[] codeInstructions = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldloc_1),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, _startRotationField),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, _middleRotationField),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, _endRotationField),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, _playerTransformsField),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, _inverseWorldRotationField),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, _rotatedObjectField),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, _getTransform),
                        new CodeInstruction(OpCodes.Call, _doNoteLook),
                    };
                    instructionList.InsertRange(i + 1, codeInstructions);
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

            if (!foundZOffset)
            {
                NoodleLogger.Log("Failed to find brfalse.s to Label21!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundLook)
            {
                NoodleLogger.Log("Failed to find bge.un to Label6!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static Vector3 DefiniteNoteJump(Vector3 original, float time)
        {
            NoodleObjectData noodleData = NoteControllerUpdate.NoodleData;
            if (noodleData != null)
            {
                AnimationHelper.GetDefinitePositionOffset(noodleData.AnimationObject, noodleData.Track, time, out Vector3? position);
                if (position.HasValue)
                {
                    Vector3 noteOffset = noodleData.NoteOffset;
                    _definitePosition = true;
                    return position.Value + noteOffset;
                }
            }

            _definitePosition = false;
            return original;
        }

        private static void DoNoteLook(
            float num2,
            Quaternion startRotation,
            Quaternion middleRotation,
            Quaternion endRotation,
            PlayerTransforms playerTransforms,
            Quaternion inverseWorldRotation,
            Transform rotatedObject,
            Transform baseTransform)
        {
            Quaternion a;
            if (num2 < 0.125f)
            {
                a = Quaternion.Slerp(baseTransform.rotation * startRotation, baseTransform.rotation * middleRotation, Mathf.Sin(num2 * Mathf.PI * 4f));
            }
            else
            {
                a = Quaternion.Slerp(baseTransform.rotation * middleRotation, baseTransform.rotation * endRotation, Mathf.Sin((num2 - 0.125f) * Mathf.PI * 2f));
            }

            Vector3 vector = playerTransforms.headWorldPos;
            vector.y = Mathf.Lerp(vector.y, baseTransform.position.y, 0.8f);
            vector = inverseWorldRotation * vector;
            Vector3 normalized = (baseTransform.position - vector).normalized;
            Quaternion b = default;
            Vector3 point = rotatedObject.up;
            b.SetLookRotation(normalized, inverseWorldRotation * point);
            rotatedObject.rotation = Quaternion.Lerp(a, b, num2 * 2f);
        }
    }
}
