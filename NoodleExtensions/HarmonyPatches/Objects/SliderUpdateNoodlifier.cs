using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using Heck.Animation;
using Heck.Deserialize;
using NoodleExtensions.Animation;
using NoodleExtensions.Managers;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.Objects;

internal class SliderUpdateNoodlifier : IAffinity, IDisposable
{
    private static readonly FieldInfo _headNoteJumpEndPos = AccessTools.Field(
        typeof(SliderMovement),
        "_headNoteJumpEndPos");

    private static readonly FieldInfo _headNoteJumpStartPos = AccessTools.Field(
        typeof(SliderMovement),
        "_headNoteJumpStartPos");

    private static readonly FieldInfo _headNoteTime = AccessTools.Field(typeof(SliderMovement), "_headNoteTime");

    private static readonly FieldInfo _inverseWorldRotation = AccessTools.Field(
        typeof(SliderMovement),
        "_inverseWorldRotation");

    private static readonly FieldInfo _jumpDuration = AccessTools.Field(typeof(SliderMovement), "_jumpDuration");
    private static readonly FieldInfo _localPosition = AccessTools.Field(typeof(SliderMovement), "_localPosition");
    private static readonly FieldInfo _tailNoteTime = AccessTools.Field(typeof(SliderMovement), "_tailNoteTime");

    private static readonly FieldInfo _timeSinceHeadNoteJump = AccessTools.Field(
        typeof(SliderMovement),
        "_timeSinceHeadNoteJump");

    private static readonly FieldInfo _worldRotation = AccessTools.Field(typeof(SliderMovement), "_worldRotation");
    private readonly AnimationHelper _animationHelper;
    private readonly AudioTimeSyncController _audioTimeSyncController;
    private readonly CutoutManager _cutoutManager;
    private readonly DeserializedData _deserializedData;
    private readonly PlayerTransforms _playerTransforms;

    private readonly CodeInstruction _sliderTimeAdjust;

    private NoodleSliderData? _noodleData;

    private SliderUpdateNoodlifier(
        [Inject(Id = ID)] DeserializedData deserializedData,
        AnimationHelper animationHelper,
        CutoutManager cutoutManager,
        AudioTimeSyncController audioTimeSyncController,
        PlayerTransforms playerTransforms)
    {
        _deserializedData = deserializedData;
        _animationHelper = animationHelper;
        _cutoutManager = cutoutManager;
        _audioTimeSyncController = audioTimeSyncController;
        _playerTransforms = playerTransforms;
        _sliderTimeAdjust = InstanceTranspilers.EmitInstanceDelegate<SliderTimeAdjustDelegate>(SliderUpdate);
    }

    private delegate void SliderTimeAdjustDelegate(
        SliderMovement instance,
        float headNoteTime,
        float tailNoteTime,
        float jumpDuration,
        ref float timeSinceHeadNoteJump,
        ref float normalizedHeadTime,
        ref float normalizedTailTime,
        ref Vector3 headNoteJumpStartPos,
        ref Vector3 headNoteJumpEndPos,
        ref Quaternion worldRotation,
        ref Quaternion inverseWorldRotation,
        ref Vector3 localPosition);

    public void Dispose()
    {
        InstanceTranspilers.DisposeDelegate(_sliderTimeAdjust);
    }

    [AffinityPrefix]
    [AffinityPatch(typeof(SliderController), nameof(SliderController.ManualUpdate))]
    private void GetData(SliderData ____sliderData)
    {
        _deserializedData.Resolve(____sliderData, out _noodleData);
    }

    private void SliderUpdate(
        SliderMovement instance,
        float headNoteTime,
        float tailNoteTime,
        float jumpDuration,
        ref float timeSinceHeadNoteJump,
        ref float normalizedHeadTime,
        ref float normalizedTailTime,
        ref Vector3 headNoteJumpStartPos,
        ref Vector3 headNoteJumpEndPos,
        ref Quaternion worldRotation,
        ref Quaternion inverseWorldRotation,
        ref Vector3 localPosition)
    {
        float duration = (jumpDuration * 0.75f) + (tailNoteTime - headNoteTime);
        float normalizedTime;
        float timeSinceTailNoteJump;
        float halfJumpDuration = jumpDuration * 0.5f;

        float? time = _noodleData?.GetTimeProperty();
        if (time.HasValue)
        {
            normalizedTime = time.Value;
            timeSinceHeadNoteJump = normalizedTime * duration;
            timeSinceTailNoteJump = (timeSinceHeadNoteJump + (headNoteTime - halfJumpDuration)) -
                                    (tailNoteTime - halfJumpDuration);
        }
        else
        {
            float songTime = _audioTimeSyncController.songTime;
            timeSinceHeadNoteJump = songTime - (headNoteTime - halfJumpDuration);
            normalizedTime = timeSinceHeadNoteJump / duration;
            timeSinceTailNoteJump = songTime - (tailNoteTime - halfJumpDuration);
        }

        normalizedHeadTime = timeSinceHeadNoteJump / jumpDuration;
        normalizedTailTime = timeSinceTailNoteJump / jumpDuration;

        Transform transform = instance.transform;
        localPosition = Vector3.zero;

        if (_noodleData != null)
        {
            IReadOnlyList<Track>? tracks = _noodleData?.Track;
            NoodleObjectData.AnimationObjectData? animationObject = _noodleData?.AnimationObject;
            if (tracks != null || animationObject != null)
            {
                normalizedTime = Math.Max(normalizedTime, 0);
                _animationHelper.GetObjectOffset(
                    animationObject,
                    tracks,
                    normalizedTime,
                    out Vector3? positionOffset,
                    out Quaternion? rotationOffset,
                    out Vector3? scaleOffset,
                    out Quaternion? localRotationOffset,
                    out float? dissolve,
                    out _,
                    out _);

                if (rotationOffset.HasValue || localRotationOffset.HasValue)
                {
                    Quaternion noodleWorldRotation = _noodleData!.InternalWorldRotation;
                    Quaternion localRotation = _noodleData.InternalLocalRotation;

                    Quaternion worldRotationQuatnerion = noodleWorldRotation;
                    if (rotationOffset.HasValue)
                    {
                        worldRotationQuatnerion *= rotationOffset.Value;
                        worldRotation = worldRotationQuatnerion;
                        inverseWorldRotation = Quaternion.Inverse(worldRotationQuatnerion);
                    }

                    worldRotationQuatnerion *= localRotation;

                    if (localRotationOffset.HasValue)
                    {
                        worldRotationQuatnerion *= localRotationOffset.Value;
                    }

                    transform.localRotation = worldRotationQuatnerion;
                }

                if (scaleOffset.HasValue)
                {
                    transform.localScale = scaleOffset.Value;
                }

                if (dissolve.HasValue)
                {
                    _cutoutManager.SliderCutoutEffects[instance].SetCutout(dissolve.Value);
                }

                _animationHelper.GetDefinitePositionOffset(
                    animationObject,
                    tracks,
                    normalizedTime,
                    out Vector3? definitePosition);
                if (definitePosition.HasValue)
                {
                    transform.localPosition = definitePosition.Value;
                    return;
                }

                if (positionOffset.HasValue)
                {
                    Vector3 startPos = _noodleData!.InternalStartPos;
                    Vector3 endPos = _noodleData.InternalEndPos;

                    Vector3 offset = positionOffset.Value;
                    headNoteJumpStartPos = startPos + offset;
                    headNoteJumpEndPos = endPos + offset;
                    localPosition = offset;
                }
            }
        }

        localPosition.z += _playerTransforms.MoveTowardsHead(
            headNoteJumpStartPos.z,
            headNoteJumpEndPos.z,
            inverseWorldRotation,
            normalizedHeadTime);
        transform.localPosition = worldRotation * localPosition;
    }

    [AffinityTranspiler]
    [AffinityPatch(typeof(SliderMovement), nameof(SliderMovement.ManualUpdate))]
    private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            /*
             * -- float songTime = this._audioTimeSyncController.songTime;
             * -- this._timeSinceHeadNoteJump = songTime - (this._headNoteTime - this._jumpDuration * 0.5f);
             * -- float num = songTime - (this._tailNoteTime - this._jumpDuration * 0.5f);
             * -- float num2 = this._timeSinceHeadNoteJump / this._jumpDuration;
             * -- float num3 = num / this._jumpDuration;
             * -- this._localPosition.z = this._playerTransforms.MoveTowardsHead(this._headNoteJumpStartPos.z, this._headNoteJumpEndPos.z, this._inverseWorldRotation, num2);
             * -- Vector3 localPosition = this._worldRotation * this._localPosition;
             * -- this._transform.localPosition = localPosition;
             * ++ SliderUpdate(this, this._headNoteTime, this._tailNoteTime, this._jumpDuration, ref this._timeSinceHeadNoteJump, ref num2, ref num3, ref this._headNoteJumpStartPos, ref this._headNoteJumpEndPos, ref this._worldRotation, ref this._inverseWorldRotation);
             */
            .Start()
            .RemoveInstructions(34)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, _headNoteTime),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, _tailNoteTime),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, _jumpDuration),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, _timeSinceHeadNoteJump),
                new CodeInstruction(OpCodes.Ldloca_S, 1),
                new CodeInstruction(OpCodes.Ldloca_S, 2),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, _headNoteJumpStartPos),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, _headNoteJumpEndPos),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, _worldRotation),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, _inverseWorldRotation),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldflda, _localPosition),
                _sliderTimeAdjust)
            .InstructionEnumeration();
    }
}
