using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using Heck.Deserialize;
using NoodleExtensions.HarmonyPatches.SmallFixes;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace NoodleExtensions.HarmonyPatches.Objects
{
    internal class SliderInitNoodlifier : IAffinity, IDisposable
    {
        private static readonly MethodInfo _noteJumpMovementSpeedGetter =
            AccessTools.PropertyGetter(typeof(IBeatmapObjectSpawnController), nameof(IBeatmapObjectSpawnController.noteJumpMovementSpeed));

        private readonly BeatmapObjectSpawnMovementData _movementData;
        private readonly DeserializedData _deserializedData;

        private readonly CodeInstruction _getNJS;

        private SliderInitNoodlifier(
            InitializedSpawnMovementData initializedSpawnMovementData,
            [Inject(Id = NoodleController.ID)] DeserializedData deserializedData)
        {
            _movementData = initializedSpawnMovementData.MovementData;
            _deserializedData = deserializedData;

            _getNJS = InstanceTranspilers.EmitInstanceDelegate<Func<float, SliderData, float>>(GetCustomNJS);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_getNJS);
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(SliderController), nameof(SliderController.Init))]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * -- float noteJumpMovementSpeed = this._beatmapObjectSpawnController.noteJumpMovementSpeed;
                 * ++ float noteJumpMovementSpeed = GetCustomNJS(this._beatmapObjectSpawnController.noteJumpMovementSpeed, sliderData);
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _noteJumpMovementSpeedGetter))
                .Repeat(n => n
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_2),
                        _getNJS))
                .InstructionEnumeration();
        }

        private float GetCustomNJS(float @default, SliderData sliderData)
        {
            _deserializedData.Resolve(sliderData, out NoodleSliderData? noodleData);
            return noodleData?.NJS ?? @default;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(SliderController), nameof(SliderController.Init))]
        private void Postfix(
            SliderController __instance,
            SliderMovement ____sliderMovement,
            MaterialPropertyBlockController ____materialPropertyBlockController,
            SliderData sliderData,
            Vector3 headNoteJumpStartPos,
            Vector3 tailNoteJumpStartPos,
            Vector3 headNoteJumpEndPos,
            Vector3 tailNoteJumpEndPos)
        {
            if (!_deserializedData.Resolve(sliderData, out NoodleSliderData? noodleData))
            {
                return;
            }

            Quaternion? worldRotationQuaternion = noodleData.WorldRotationQuaternion;
            Quaternion? localRotationQuaternion = noodleData.LocalRotationQuaternion;

            Transform transform = __instance.transform;

            Quaternion localRotation = Quaternion.identity;
            if (worldRotationQuaternion.HasValue || localRotationQuaternion.HasValue)
            {
                if (localRotationQuaternion.HasValue)
                {
                    localRotation = localRotationQuaternion.Value;
                }

                if (worldRotationQuaternion.HasValue)
                {
                    Quaternion quatVal = worldRotationQuaternion.Value;
                    Quaternion inverseWorldRotation = Quaternion.Inverse(quatVal);
                    ____sliderMovement._worldRotation = quatVal;
                    ____sliderMovement._inverseWorldRotation = inverseWorldRotation;

                    quatVal *= localRotation;

                    transform.localRotation = quatVal;
                }
                else
                {
                    transform.localRotation *= localRotation;
                }
            }

            transform.localScale = Vector3.one;

            noodleData.InternalStartPos = headNoteJumpStartPos;
            noodleData.InternalEndPos = headNoteJumpEndPos;
            noodleData.InternalWorldRotation = ____sliderMovement._worldRotation;
            noodleData.InternalLocalRotation = localRotation;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(SliderController), nameof(SliderController.IsNoteStartOfThisSlider))]
        private bool CompareCustomData(SliderData ____sliderData, NoteData noteData, ref bool __result)
        {
            if (!Mathf.Approximately(noteData.time, ____sliderData.time) ||
                noteData.colorType != ____sliderData.colorType)
            {
                return false;
            }

            if (!_deserializedData.Resolve(____sliderData, out NoodleSliderData? noodleSliderData) ||
                !_deserializedData.Resolve(noteData, out NoodleBaseNoteData? noodleNoteData))
            {
                return true;
            }

            int offset = _movementData.noteLinesCount / 2;
            float headIndex = noodleSliderData.StartX + offset ?? ____sliderData.headLineIndex;
            float noteIndex = noodleNoteData.StartX + offset ?? noteData.lineIndex;
            float headLayer = noodleSliderData.StartY ?? (float)____sliderData.headLineLayer;
            float noteLayer = noodleNoteData.StartY ?? (float)noteData.noteLineLayer;

            __result = Mathf.Approximately(headIndex, noteIndex) && Mathf.Approximately(headLayer, noteLayer);
            return false;
        }
    }
}
