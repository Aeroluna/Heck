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

namespace NoodleExtensions.HarmonyPatches.Objects;

internal class SliderInitNoodlifier : IAffinity, IDisposable
{
#if !LATEST
    private static readonly MethodInfo _noteJumpMovementSpeedGetter =
        AccessTools.PropertyGetter(
            typeof(IBeatmapObjectSpawnController),
            nameof(IBeatmapObjectSpawnController.noteJumpMovementSpeed));
#endif

    private readonly DeserializedData _deserializedData;

    private readonly CodeInstruction _getNjs;

    private readonly BeatmapObjectSpawnMovementData _movementData;

    private SliderInitNoodlifier(
        InitializedSpawnMovementData initializedSpawnMovementData,
        [Inject(Id = NoodleController.ID)] DeserializedData deserializedData)
    {
        _movementData = initializedSpawnMovementData.MovementData;
        _deserializedData = deserializedData;

        _getNjs = InstanceTranspilers.EmitInstanceDelegate<Func<float, SliderData, float>>(GetCustomNjs);
    }

    public void Dispose()
    {
        InstanceTranspilers.DisposeDelegate(_getNjs);
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

    private float GetCustomNjs(float @default, SliderData sliderData)
    {
        _deserializedData.Resolve(sliderData, out NoodleSliderData? noodleData);
        return noodleData?.Njs ?? @default;
    }

    [AffinityPostfix]
    [AffinityPatch(typeof(SliderController), nameof(SliderController.Init))]
    private void Postfix(
        SliderController __instance,
        SliderMovement ____sliderMovement,
        MaterialPropertyBlockController ____materialPropertyBlockController,
#if !LATEST
        Vector3 headNoteJumpStartPos,
        Vector3 tailNoteJumpStartPos,
        Vector3 headNoteJumpEndPos,
        Vector3 tailNoteJumpEndPos,
#endif
        SliderData sliderData)
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
                ____sliderMovement._worldRotation = quatVal;
#if !LATEST
                Quaternion inverseWorldRotation = Quaternion.Inverse(quatVal);
                ____sliderMovement._inverseWorldRotation = inverseWorldRotation;
#endif

                quatVal *= localRotation;

                transform.localRotation = quatVal;
            }
            else
            {
                transform.localRotation *= localRotation;
            }
        }

        Vector3 scale = (noodleData.ScaleX != null || noodleData.ScaleY != null || noodleData.ScaleZ != null)
            ? new Vector3(noodleData.ScaleX ?? 1, noodleData.ScaleY ?? 1, noodleData.ScaleZ ?? 1)
            : Vector3.one;
        transform.localScale = scale;
        noodleData.InternalScale = scale;

#if !LATEST
        noodleData.InternalStartPos = headNoteJumpStartPos;
        noodleData.InternalEndPos = headNoteJumpEndPos;
#endif
        noodleData.InternalWorldRotation = ____sliderMovement._worldRotation;
        noodleData.InternalLocalRotation = localRotation;
    }

#if !LATEST
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
            .Repeat(
                n => n
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_2),
                        _getNjs))
            .InstructionEnumeration();
    }
#endif
}
