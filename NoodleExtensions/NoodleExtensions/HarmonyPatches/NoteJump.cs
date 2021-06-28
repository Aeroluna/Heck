namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;
    using IPA.Utilities;
    using NoodleExtensions.Animation;
    using UnityEngine;

    [HeckPatch(typeof(NoteJump))]
    [HeckPatch("ManualUpdate")]
    internal static class NoteJumpManualUpdate
    {
        private static readonly FieldInfo _jumpDurationField = AccessTools.Field(typeof(NoteJump), "_jumpDuration");
        private static readonly MethodInfo _noteJumpTimeAdjust = AccessTools.Method(typeof(NoteJumpManualUpdate), nameof(NoteJumpTimeAdjust));
        private static readonly FieldInfo _localPositionField = AccessTools.Field(typeof(NoteJump), "_localPosition");
        private static readonly MethodInfo _definiteNoteJump = AccessTools.Method(typeof(NoteJumpManualUpdate), nameof(DefiniteNoteJump));
        private static readonly FieldInfo _definitePositionField = AccessTools.Field(typeof(NoteJumpManualUpdate), nameof(_definitePosition));
        private static readonly MethodInfo _getTransform = AccessTools.PropertyGetter(typeof(Component), nameof(Component.transform));
        private static readonly MethodInfo _doNoteLook = AccessTools.Method(typeof(NoteJumpManualUpdate), nameof(DoNoteLook));
        private static readonly FieldInfo _startRotationField = AccessTools.Field(typeof(NoteJump), "_startRotation");
        private static readonly FieldInfo _middleRotationField = AccessTools.Field(typeof(NoteJump), "_middleRotation");
        private static readonly FieldInfo _endRotationField = AccessTools.Field(typeof(NoteJump), "_endRotation");
        private static readonly FieldInfo _playerTransformsField = AccessTools.Field(typeof(NoteJump), "_playerTransforms");
        private static readonly FieldInfo _rotatedObjectField = AccessTools.Field(typeof(NoteJump), "_rotatedObject");
        private static readonly FieldInfo _inverseWorldRotationField = AccessTools.Field(typeof(NoteJump), "_inverseWorldRotation");
        private static readonly FieldAccessor<PlayerTransforms, Transform>.Accessor _headTransformAccessor = FieldAccessor<PlayerTransforms, Transform>.GetAccessor("_headTransform");

        // This field is used by reflection
#pragma warning disable CS0414
        private static bool _definitePosition = false;
#pragma warning restore CS0414

        internal static float NoteJumpTimeAdjust(float original, float jumpDuration)
        {
            NoodleObjectData? noodleData = NoteControllerUpdate.NoodleData;
            if (noodleData != null)
            {
                float? time = (float?)Heck.Animation.AnimationHelper.TryGetProperty(noodleData.Track, Plugin.TIME);
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
                    CodeInstruction[] codeInstructions = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, _jumpDurationField),
                        new CodeInstruction(OpCodes.Call, _noteJumpTimeAdjust),
                    };

                    instructionList.InsertRange(i, codeInstructions);
                }

                if (!foundFinalPosition &&
                    instructionList[i].opcode == OpCodes.Stind_R4)
                {
                    foundFinalPosition = true;
                    CodeInstruction[] codeInstructions = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, _localPositionField),
                        new CodeInstruction(OpCodes.Ldloc_1),
                        new CodeInstruction(OpCodes.Call, _definiteNoteJump),
                        new CodeInstruction(OpCodes.Stfld, _localPositionField),
                    };
                    instructionList.InsertRange(i + 2, codeInstructions);
                }

                // temporarily replacing label checks
                if (!foundZOffset &&
                    instructionList[i].opcode == OpCodes.Ldfld &&
                    ((FieldInfo)instructionList[i].operand).Name == "_endDistanceOffset")
                {
                    foundZOffset = true;

                    // Add addition check to our quirky little variable to skip end position offset when we are using definitePosition
                    CodeInstruction[] codeInstructions = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldsfld, _definitePositionField),
                        new CodeInstruction(OpCodes.Brtrue_S, instructionList[i - 20].operand),
                    };
                    instructionList.InsertRange(i - 19, codeInstructions);
                }

                // Override all the rotation stuff
                if (!foundLook &&
                    instructionList[i].opcode == OpCodes.Ldfld &&
                    ((FieldInfo)instructionList[i].operand).Name == "_startRotation")
                {
                    Label label = (Label)instructionList[i - 5].operand;
                    int endIndex = instructionList.FindIndex(n => n.labels.Contains(label));

                    foundLook = true;

                    instructionList.RemoveRange(i - 4, endIndex - i + 4);

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
                        new CodeInstruction(OpCodes.Ldfld, _rotatedObjectField),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, _getTransform),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, _inverseWorldRotationField),
                        new CodeInstruction(OpCodes.Call, _doNoteLook),
                    };
                    instructionList.InsertRange(i - 4, codeInstructions);
                }
            }

            if (!foundTime)
            {
                Plugin.Logger.Log("Failed to find stloc.0!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundFinalPosition)
            {
                Plugin.Logger.Log("Failed to find stind.r4!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundZOffset)
            {
                Plugin.Logger.Log("Failed to find brfalse.s to Label21!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundLook)
            {
                Plugin.Logger.Log("Failed to find bge.un to Label6!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static Vector3 DefiniteNoteJump(Vector3 original, float time)
        {
            NoodleObjectData? noodleData = NoteControllerUpdate.NoodleData;
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

        // Performs all note look rotation from world space
        // Never want to touch this again....
        private static void DoNoteLook(
            float num2,
            Quaternion startRotation,
            Quaternion middleRotation,
            Quaternion endRotation,
            PlayerTransforms playerTransforms,
            Transform rotatedObject,
            Transform baseTransform,
            Quaternion inverseWorldRotation)
        {
            NoodleNoteData? noodleData = (NoodleNoteData?)NoteControllerUpdate.NoodleData;
            if (noodleData != null && noodleData.DisableLook)
            {
                rotatedObject.localRotation = endRotation;
                return;
            }

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

            // idk whats happening anymore
            Quaternion worldRot = inverseWorldRotation;
            if (baseTransform.parent != null)
            {
                // Handle parenting
                worldRot *= Quaternion.Inverse(baseTransform.parent.rotation);
            }

            // This line but super complicated so that "y" = "originTransform.up"
            // vector.y = Mathf.Lerp(vector.y, this._localPosition.y, 0.8f);
            Transform headTransform = _headTransformAccessor(ref playerTransforms);
            Quaternion inverse = Quaternion.Inverse(worldRot);
            Vector3 upVector = inverse * Vector3.up;
            float baseUpMagnitude = Vector3.Dot(worldRot * baseTransform.position, Vector3.up);
            float headUpMagnitude = Vector3.Dot(worldRot * headTransform.position, Vector3.up);
            float mult = Mathf.Lerp(headUpMagnitude, baseUpMagnitude, 0.8f) - headUpMagnitude;
            vector += upVector * mult;

            // more wtf
            Vector3 normalized = baseTransform.rotation * (worldRot * (baseTransform.position - vector).normalized);

            Quaternion b = Quaternion.LookRotation(normalized, rotatedObject.up);
            rotatedObject.rotation = Quaternion.Lerp(a, b, num2 * 2f);
        }
    }
}
