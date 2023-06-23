using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using NoodleExtensions.Animation;
using SiraUtil.Affinity;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.Objects
{
    internal class NoteJumpNoodlifier : IAffinity, IDisposable
    {
        private static readonly FieldInfo _threeQuartersMarkReportedField = AccessTools.Field(typeof(NoteJump), "_threeQuartersMarkReported");
        private static readonly MethodInfo _localRotationSetter = AccessTools.PropertySetter(typeof(Transform), nameof(Transform.localRotation));

        private static readonly FieldInfo _missedTimeField = AccessTools.Field(typeof(NoteJump), "_missedTime");
        private static readonly FieldInfo _beatTimeField = AccessTools.Field(typeof(NoteJump), "_beatTime");
        private static readonly FieldInfo _jumpDurationField = AccessTools.Field(typeof(NoteJump), "_jumpDuration");
        private static readonly FieldInfo _localPositionField = AccessTools.Field(typeof(NoteJump), "_localPosition");
        private static readonly MethodInfo _getTransform = AccessTools.PropertyGetter(typeof(Component), nameof(Component.transform));
        private static readonly FieldInfo _startRotationField = AccessTools.Field(typeof(NoteJump), "_startRotation");
        private static readonly FieldInfo _middleRotationField = AccessTools.Field(typeof(NoteJump), "_middleRotation");
        private static readonly FieldInfo _endRotationField = AccessTools.Field(typeof(NoteJump), "_endRotation");
        private static readonly FieldInfo _playerTransformsField = AccessTools.Field(typeof(NoteJump), "_playerTransforms");
        private static readonly FieldInfo _rotatedObjectField = AccessTools.Field(typeof(NoteJump), "_rotatedObject");
        private static readonly FieldInfo _inverseWorldRotationField = AccessTools.Field(typeof(NoteJump), "_inverseWorldRotation");

        private static readonly MethodInfo _noteMissedTimeAdjust = AccessTools.Method(typeof(NoteJumpNoodlifier), nameof(NoteMissedTimeAdjust));

        private readonly CodeInstruction _doNoteLook;
        private readonly CodeInstruction _noteJumpTimeAdjust;
        private readonly CodeInstruction _definiteNoteJump;
        private readonly CodeInstruction _getDefinitePosition;
        private readonly NoteUpdateNoodlifier _noteUpdateNoodlifier;
        private readonly AnimationHelper _animationHelper;

        private bool _definitePosition;

        private NoteJumpNoodlifier(NoteUpdateNoodlifier noteUpdateNoodlifier, AnimationHelper animationHelper)
        {
            _noteUpdateNoodlifier = noteUpdateNoodlifier;
            _animationHelper = animationHelper;
            _doNoteLook = InstanceTranspilers.EmitInstanceDelegate<Action<float, Quaternion, Quaternion, Quaternion, PlayerTransforms, Transform, Transform, Quaternion>>(DoNoteLook);
            _noteJumpTimeAdjust = InstanceTranspilers.EmitInstanceDelegate<Func<float, float, float>>(NoteJumpTimeAdjust);
            _definiteNoteJump = InstanceTranspilers.EmitInstanceDelegate<Func<Vector3, float, Vector3>>(DefiniteNoteJump);
            _getDefinitePosition = InstanceTranspilers.EmitInstanceDelegate<Func<bool>>(() => _definitePosition);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_doNoteLook);
            InstanceTranspilers.DisposeDelegate(_noteJumpTimeAdjust);
            InstanceTranspilers.DisposeDelegate(_definiteNoteJump);
            InstanceTranspilers.DisposeDelegate(_getDefinitePosition);
        }

        private static float NoteMissedTimeAdjust(float beatTime, float jumpDuration, float num)
        {
            return num + (beatTime - (jumpDuration * 0.5f));
        }

        private float NoteJumpTimeAdjust(float original, float jumpDuration)
        {
            float? time = _noteUpdateNoodlifier.NoodleData?.GetTimeProperty();
            if (time.HasValue)
            {
                return time.Value * jumpDuration;
            }

            return original;
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(NoteJump), nameof(NoteJump.ManualUpdate))]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher codeMatcher = new(instructions);

            // CodeMatcher needs some better label manipulating methods
            // replace rotation stuff
            /*
             *
             * if (num2 < 0.5f)
             * {
             * --     Quaternion quaternion;
             * --     if (num2 < 0.125f)
             * --     {
             * --       quaternion = Quaternion.Slerp(this._startRotation, this._middleRotation, Mathf.Sin(num2 * 3.1415927f * 4f));
             * --     }
             * --     else
             * --     {
             * --       quaternion = Quaternion.Slerp(this._middleRotation, this._endRotation, Mathf.Sin((num2 - 0.125f) * 3.1415927f * 2f));
             * --     }
             * --     if (this._rotateTowardsPlayer)
             * --     {
             * --       Vector3 vector = this._playerTransforms.headPseudoLocalPos;
             * --       vector.y = Mathf.Lerp(vector.y, this._localPosition.y, 0.8f);
             * --       vector = this._inverseWorldRotation * vector;
             * --       Vector3 normalized = (this._localPosition - vector).normalized;
             * --       Quaternion b = default(Quaternion);
             * --       Vector3 point = this._playerSpaceConvertor.worldToPlayerSpaceRotation * this._rotatedObject.up;
             * --       b.SetLookRotation(normalized, this._inverseWorldRotation * point);
             * --       this._rotatedObject.localRotation = Quaternion.Lerp(quaternion, b, num2 * 2f);
             * --     }
             * --     else
             * --     {
             * --       this._rotatedObject.localRotation = quaternion;
             * --     }
             * ++ DoNoteLook(num2, this._startRotation, this._middleRotation, this._endRotation, this._playerTransforms, this._rotatedObject, base.transform, this._inverseWorldRotation);
             * }
             */
            codeMatcher
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Callvirt, _localRotationSetter))
                .Advance(1)
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Callvirt, _localRotationSetter))
                .Advance(1);
            int endPos = codeMatcher.Pos;
            object label = codeMatcher.Labels.First();
            codeMatcher
                .MatchBack(
                    false,
                    new CodeMatch(null, label))
                .Advance(1)
                .RemoveInstructions(endPos - codeMatcher.Pos)
                .InsertAndAdvance(
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
                    _doNoteLook)

            // Add addition check to our quirky little variable to skip end position offset when we are using definitePosition
            /*
             * -- if (this._threeQuartersMarkReported)
             * ++ if (this._threeQuartersMarkReported && !_definitePosition)
             */
                .MatchForward(
                    true,
                    new CodeMatch(OpCodes.Ldfld, _threeQuartersMarkReportedField),
                    new CodeMatch(OpCodes.Brfalse));
            label = codeMatcher.Operand;
            codeMatcher
                .Advance(1)
                .Insert(
                    _getDefinitePosition,
                    new CodeInstruction(OpCodes.Brtrue_S, label))
                .Start();

            return codeMatcher

                // time adjust
                /*
                 * -- float num = songTime - (this._beatTime - this._jumpDuration * 0.5f);
                 * ++ float num = NoteJumpTimeAdjust(songTime - (this._beatTime - this._jumpDuration * 0.5f), this._jumpDuration);
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_0))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _jumpDurationField),
                    _noteJumpTimeAdjust)

                // time adjust miss time
                /*
                 * -- if (songTime >= this._missedTime && !this._missedMarkReported)
                 * ++ if (NoteMissedTimeAdjust(_beatTime, _jumpDuration, num) >= this._missedTime && !this._missedMarkReported)
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, _missedTimeField))
                .Advance(-1)
                .SetOpcodeAndAdvance(OpCodes.Pop)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _beatTimeField),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _jumpDurationField),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, _noteMissedTimeAdjust),
                    new CodeInstruction(OpCodes.Ldarg_0))

                // final position
                /*
                 *      this._localPosition.y = this._localPosition.y + num3 * this._yAvoidance;
                 * }
                 * ++ this._localPosition = DefiniteNoteJump(this._localPosition, num2);
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Stind_R4))
                .Advance(2)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _localPositionField),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    _definiteNoteJump,
                    new CodeInstruction(OpCodes.Stfld, _localPositionField))

                .InstructionEnumeration();
        }

        private Vector3 DefiniteNoteJump(Vector3 original, float time)
        {
            NoodleObjectData? noodleData = _noteUpdateNoodlifier.NoodleData;
            if (noodleData != null)
            {
                _animationHelper.GetDefinitePositionOffset(noodleData.AnimationObject, noodleData.Track, time, out Vector3? position);
                if (position.HasValue)
                {
                    _definitePosition = true;
                    return position.Value + noodleData.InternalNoteOffset;
                }
            }

            _definitePosition = false;
            return original;
        }

        // Performs all note look rotation from world space
        // Never want to touch this again....
        private void DoNoteLook(
            float num2,
            Quaternion startRotation,
            Quaternion middleRotation,
            Quaternion endRotation,
            PlayerTransforms playerTransforms,
            Transform rotatedObject,
            Transform baseTransform,
            Quaternion inverseWorldRotation)
        {
            NoodleBaseNoteData? noodleData = _noteUpdateNoodlifier.NoodleData;
            if (noodleData is { DisableLook: true })
            {
                rotatedObject.localRotation = endRotation;
                return;
            }

            Quaternion rotation = baseTransform.rotation;
            Quaternion a = num2 < 0.125f
                ? Quaternion.Slerp(rotation * startRotation, rotation * middleRotation, Mathf.Sin(num2 * Mathf.PI * 4f))
                : Quaternion.Slerp(rotation * middleRotation, rotation * endRotation, Mathf.Sin((num2 - 0.125f) * Mathf.PI * 2f));

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
            Transform headTransform = playerTransforms._headTransform;
            Quaternion inverse = Quaternion.Inverse(worldRot);
            Vector3 upVector = inverse * Vector3.up;
            Vector3 position = baseTransform.position;
            float baseUpMagnitude = Vector3.Dot(worldRot * position, Vector3.up);
            float headUpMagnitude = Vector3.Dot(worldRot * headTransform.position, Vector3.up);
            float mult = Mathf.Lerp(headUpMagnitude, baseUpMagnitude, 0.8f) - headUpMagnitude;
            vector += upVector * mult;

            // more wtf
            Vector3 normalized = baseTransform.rotation * (worldRot * (position - vector).normalized);

            Quaternion b = Quaternion.LookRotation(normalized, rotatedObject.up);
            rotatedObject.rotation = Quaternion.Lerp(a, b, num2 * 2f);
        }
    }
}
