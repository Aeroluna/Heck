namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using IPA.Utilities;
    using NoodleExtensions.Animation;
    using UnityEngine;
    using static NoodleExtensions.Plugin;

    [NoodlePatch(typeof(NoteController))]
    [NoodlePatch("Init")]
    internal static class NoteControllerInit
    {
        internal static readonly FieldAccessor<NoteMovement, NoteJump>.Accessor _noteJumpAccessor = FieldAccessor<NoteMovement, NoteJump>.GetAccessor("_jump");
        internal static readonly FieldAccessor<NoteMovement, NoteFloorMovement>.Accessor _noteFloorMovementAccessor = FieldAccessor<NoteMovement, NoteFloorMovement>.GetAccessor("_floorMovement");

        internal static readonly FieldAccessor<NoteFloorMovement, Quaternion>.Accessor _worldRotationFloorAccessor = FieldAccessor<NoteFloorMovement, Quaternion>.GetAccessor("_worldRotation");
        internal static readonly FieldAccessor<NoteFloorMovement, Quaternion>.Accessor _inverseWorldRotationFloorAccessor = FieldAccessor<NoteFloorMovement, Quaternion>.GetAccessor("_inverseWorldRotation");

        internal static readonly FieldAccessor<NoteJump, Quaternion>.Accessor _worldRotationJumpAccessor = FieldAccessor<NoteJump, Quaternion>.GetAccessor("_worldRotation");
        internal static readonly FieldAccessor<NoteJump, Quaternion>.Accessor _inverseWorldRotationJumpAccessor = FieldAccessor<NoteJump, Quaternion>.GetAccessor("_inverseWorldRotation");
        private static readonly FieldAccessor<NoteJump, Quaternion>.Accessor _endRotationAccessor = FieldAccessor<NoteJump, Quaternion>.GetAccessor("_endRotation");
        private static readonly FieldAccessor<NoteJump, Quaternion>.Accessor _middleRotationAccessor = FieldAccessor<NoteJump, Quaternion>.GetAccessor("_middleRotation");
        private static readonly FieldAccessor<NoteJump, Vector3[]>.Accessor _randomRotationsAccessor = FieldAccessor<NoteJump, Vector3[]>.GetAccessor("_randomRotations");
        private static readonly FieldAccessor<NoteJump, int>.Accessor _randomRotationIdxAccessor = FieldAccessor<NoteJump, int>.GetAccessor("_randomRotationIdx");

        private static readonly MethodInfo _getFlipYSide = SymbolExtensions.GetMethodInfo(() => GetFlipYSide(null, 0));

        private static readonly MethodInfo _noteControllerUpdate = typeof(NoteController).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo _gameNoteControllerUpdate = typeof(GameNoteController).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(NoteController __instance, NoteData noteData, NoteMovement ____noteMovement, Vector3 moveStartPos, Vector3 moveEndPos, Vector3 jumpEndPos)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            if (noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;

                float? cutDir = (float?)Trees.at(dynData, CUTDIRECTION);

                NoteJump noteJump = _noteJumpAccessor(ref ____noteMovement);
                NoteFloorMovement floorMovement = _noteFloorMovementAccessor(ref ____noteMovement);

                if (cutDir.HasValue)
                {
                    Quaternion cutQuaternion = Quaternion.Euler(0, 0, cutDir.Value);
                    _endRotationAccessor(ref noteJump) = cutQuaternion;
                    Vector3 vector = cutQuaternion.eulerAngles;
                    vector += _randomRotationsAccessor(ref noteJump)[_randomRotationIdxAccessor(ref noteJump)] * 20;
                    Quaternion midrotation = Quaternion.Euler(vector);
                    _middleRotationAccessor(ref noteJump) = midrotation;
                }

                dynamic rotation = Trees.at(dynData, ROTATION);
                IEnumerable<float> localrot = ((List<object>)Trees.at(dynData, LOCALROTATION))?.Select(n => Convert.ToSingle(n));

                Transform transform = __instance.transform;

                Quaternion localRotation = _quaternionIdentity;
                if (rotation != null || localRotation != null)
                {
                    if (localrot != null)
                    {
                        localRotation = Quaternion.Euler(localrot.ElementAt(0), localrot.ElementAt(1), localrot.ElementAt(2));
                    }

                    Quaternion worldRotationQuatnerion;
                    if (rotation != null)
                    {
                        if (rotation is List<object> list)
                        {
                            IEnumerable<float> rot = list?.Select(n => Convert.ToSingle(n));
                            worldRotationQuatnerion = Quaternion.Euler(rot.ElementAt(0), rot.ElementAt(1), rot.ElementAt(2));
                        }
                        else
                        {
                            worldRotationQuatnerion = Quaternion.Euler(0, (float)rotation, 0);
                        }

                        Quaternion inverseWorldRotation = Quaternion.Euler(-worldRotationQuatnerion.eulerAngles);
                        _worldRotationJumpAccessor(ref noteJump) = worldRotationQuatnerion;
                        _inverseWorldRotationJumpAccessor(ref noteJump) = inverseWorldRotation;
                        _worldRotationFloorAccessor(ref floorMovement) = worldRotationQuatnerion;
                        _inverseWorldRotationFloorAccessor(ref floorMovement) = inverseWorldRotation;

                        worldRotationQuatnerion *= localRotation;

                        transform.localRotation = worldRotationQuatnerion;
                    }
                    else
                    {
                        transform.localRotation *= localRotation;
                    }
                }

                transform.localScale = Vector3.one; // This is a fix for animation due to notes being recycled

                Track track = AnimationHelper.GetTrack(dynData);
                if (track != null && ParentObject.Controller != null)
                {
                    ParentObject parentObject = ParentObject.Controller.GetParentObjectTrack(track);
                    if (parentObject != null)
                    {
                        parentObject.ParentToObject(transform);
                    }
                    else
                    {
                        ParentObject.ResetTransformParent(transform);
                    }
                }
                else
                {
                    ParentObject.ResetTransformParent(transform);
                }

                dynData.moveStartPos = moveStartPos;
                dynData.moveEndPos = moveEndPos;
                dynData.jumpEndPos = jumpEndPos;
                dynData.worldRotation = __instance.worldRotation;
                dynData.localRotation = localRotation;
            }

            if (__instance is GameNoteController)
            {
                _gameNoteControllerUpdate.Invoke(__instance, null);
            }
            else
            {
                _noteControllerUpdate.Invoke(__instance, null);
            }
        }

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

                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, _getFlipYSide));
                    instructionList.Insert(i - 2, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i - 1, new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(NoteController), "_noteData")));
                }
            }

            if (!foundFlipYSide)
            {
                NoodleLogger.Log("Failed to find Get_flipYSide call!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static float GetFlipYSide(NoteData noteData, float @default)
        {
            float output = @default;
            if (noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;

                float? flipYSide = (float?)Trees.at(dynData, "flipYSide");
                if (flipYSide.HasValue)
                {
                    output = flipYSide.Value;
                }
            }

            return output;
        }
    }

    [NoodlePatch(typeof(NoteController))]
    [NoodlePatch("Update")]
    internal static class NoteControllerUpdate
    {
        internal static readonly FieldAccessor<NoteFloorMovement, Vector3>.Accessor _floorEndPosAccessor = FieldAccessor<NoteFloorMovement, Vector3>.GetAccessor("_endPos");
        private static readonly FieldAccessor<NoteFloorMovement, Vector3>.Accessor _floorStartPosAccessor = FieldAccessor<NoteFloorMovement, Vector3>.GetAccessor("_startPos");
        private static readonly FieldAccessor<NoteJump, Vector3>.Accessor _jumpStartPosAccessor = FieldAccessor<NoteJump, Vector3>.GetAccessor("_startPos");
        private static readonly FieldAccessor<NoteJump, Vector3>.Accessor _jumpEndPosAccessor = FieldAccessor<NoteJump, Vector3>.GetAccessor("_endPos");

        private static readonly FieldAccessor<NoteJump, AudioTimeSyncController>.Accessor _audioTimeSyncControllerAccessor = FieldAccessor<NoteJump, AudioTimeSyncController>.GetAccessor("_audioTimeSyncController");
        private static readonly FieldAccessor<NoteJump, float>.Accessor _jumpDurationAccessor = FieldAccessor<NoteJump, float>.GetAccessor("_jumpDuration");

        private static readonly FieldAccessor<BaseNoteVisuals, CutoutAnimateEffect>.Accessor _noteCutoutAnimateEffectAccessor = FieldAccessor<BaseNoteVisuals, CutoutAnimateEffect>.GetAccessor("_cutoutAnimateEffect");
        private static readonly FieldAccessor<CutoutAnimateEffect, CutoutEffect[]>.Accessor _cutoutEffectAccessor = FieldAccessor<CutoutAnimateEffect, CutoutEffect[]>.GetAccessor("_cuttoutEffects");

        private static readonly FieldAccessor<GameNoteController, BoxCuttableBySaber>.Accessor _gameNoteBigCuttableAccessor = FieldAccessor<GameNoteController, BoxCuttableBySaber>.GetAccessor("_bigCuttableBySaber");
        private static readonly FieldAccessor<GameNoteController, BoxCuttableBySaber>.Accessor _gameNoteSmallCuttableAccessor = FieldAccessor<GameNoteController, BoxCuttableBySaber>.GetAccessor("_smallCuttableBySaber");
        private static readonly FieldAccessor<BombNoteController, CuttableBySaber>.Accessor _bombNoteCuttableAccessor = FieldAccessor<BombNoteController, CuttableBySaber>.GetAccessor("_cuttableBySaber");

        internal static CustomNoteData CustomNoteData { get; private set; }

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Prefix(NoteController __instance, NoteData ____noteData, NoteMovement ____noteMovement)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            if (____noteData is CustomNoteData customData)
            {
                CustomNoteData = customData;

                dynamic dynData = customData.customData;

                Track track = Trees.at(dynData, "track");
                dynamic animationObject = Trees.at(dynData, "_animation");
                if (track != null || animationObject != null)
                {
                    NoteJump noteJump = NoteControllerInit._noteJumpAccessor(ref ____noteMovement);
                    NoteFloorMovement floorMovement = NoteControllerInit._noteFloorMovementAccessor(ref ____noteMovement);

                    // idk i just copied base game time
                    float jumpDuration = _jumpDurationAccessor(ref noteJump);
                    float elapsedTime = _audioTimeSyncControllerAccessor(ref noteJump).songTime - (____noteData.time - (jumpDuration * 0.5f));
                    elapsedTime = NoteJumpManualUpdate.NoteJumpTimeAdjust(elapsedTime, jumpDuration);
                    float normalTime = elapsedTime / jumpDuration;

                    AnimationHelper.GetObjectOffset(animationObject, track, normalTime, out Vector3? positionOffset, out Quaternion? rotationOffset, out Vector3? scaleOffset, out Quaternion? localRotationOffset, out float? dissolve, out float? dissolveArrow, out float? cuttable);

                    if (positionOffset.HasValue)
                    {
                        Vector3 moveStartPos = Trees.at(dynData, "moveStartPos");
                        Vector3 moveEndPos = Trees.at(dynData, "moveEndPos");
                        Vector3 jumpEndPos = Trees.at(dynData, "jumpEndPos");

                        Vector3 offset = positionOffset.Value;
                        _floorStartPosAccessor(ref floorMovement) = moveStartPos + offset;
                        _floorEndPosAccessor(ref floorMovement) = moveEndPos + offset;
                        _jumpStartPosAccessor(ref noteJump) = moveEndPos + offset;
                        _jumpEndPosAccessor(ref noteJump) = jumpEndPos + offset;
                    }

                    Transform transform = __instance.transform;

                    if (rotationOffset.HasValue || localRotationOffset.HasValue)
                    {
                        Quaternion worldRotation = Trees.at(dynData, "worldRotation");
                        Quaternion localRotation = Trees.at(dynData, "localRotation");

                        Quaternion worldRotationQuatnerion = worldRotation;
                        if (rotationOffset.HasValue)
                        {
                            worldRotationQuatnerion *= rotationOffset.Value;
                            Quaternion inverseWorldRotation = Quaternion.Euler(-worldRotationQuatnerion.eulerAngles);
                            NoteControllerInit._worldRotationJumpAccessor(ref noteJump) = worldRotationQuatnerion;
                            NoteControllerInit._inverseWorldRotationJumpAccessor(ref noteJump) = inverseWorldRotation;
                            NoteControllerInit._worldRotationFloorAccessor(ref floorMovement) = worldRotationQuatnerion;
                            NoteControllerInit._inverseWorldRotationFloorAccessor(ref floorMovement) = inverseWorldRotation;
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
                        CutoutEffect cutoutEffect = Trees.at(dynData, "cutoutEffect");
                        if (cutoutEffect == null)
                        {
                            BaseNoteVisuals baseNoteVisuals = __instance.gameObject.GetComponent<BaseNoteVisuals>();
                            CutoutAnimateEffect cutoutAnimateEffect = _noteCutoutAnimateEffectAccessor(ref baseNoteVisuals);
                            CutoutEffect[] cutoutEffects = _cutoutEffectAccessor(ref cutoutAnimateEffect);
                            cutoutEffect = cutoutEffects.First(n => n.name != "NoteArrow"); // 1.11 NoteArrow has been added to the CutoutAnimateEffect and we don't want that
                            dynData.cutoutAnimateEffect = cutoutEffect;
                        }

                        cutoutEffect.SetCutout(1 - dissolve.Value);
                    }

                    if (dissolveArrow.HasValue && __instance.noteData.noteType != NoteType.Bomb)
                    {
                        DisappearingArrowController disappearingArrowController = Trees.at(dynData, "disappearingArrowController");
                        if (disappearingArrowController == null)
                        {
                            disappearingArrowController = __instance.gameObject.GetComponent<DisappearingArrowController>();
                            dynData.disappearingArrowController = disappearingArrowController;
                        }

                        disappearingArrowController.SetArrowTransparency(dissolveArrow.Value);
                    }

                    if (cuttable.HasValue)
                    {
                        bool enabled = cuttable.Value >= 1;

                        switch (__instance)
                        {
                            case GameNoteController gameNoteController:
                                BoxCuttableBySaber bigCuttableBySaber = _gameNoteBigCuttableAccessor(ref gameNoteController);
                                if (bigCuttableBySaber.enabled != enabled)
                                {
                                    bigCuttableBySaber.enabled = enabled;
                                    _gameNoteSmallCuttableAccessor(ref gameNoteController).enabled = enabled;
                                }

                                break;

                            case BombNoteController bombNoteController:
                                CuttableBySaber boxCuttableBySaber = _bombNoteCuttableAccessor(ref bombNoteController);
                                if (boxCuttableBySaber.enabled != enabled)
                                {
                                    boxCuttableBySaber.enabled = enabled;
                                }

                                break;
                        }
                    }
                }
            }
        }
    }
}
