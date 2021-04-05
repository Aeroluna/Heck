namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using IPA.Utilities;
    using NoodleExtensions.Animation;
    using UnityEngine;
    using static NoodleExtensions.NoodleObjectDataManager;
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

        private static void Postfix(NoteController __instance, NoteData noteData, NoteMovement ____noteMovement, Vector3 moveStartPos, Vector3 moveEndPos, Vector3 jumpEndPos)
        {
            if (__instance is MultiplayerConnectedPlayerNoteController)
            {
                return;
            }

            NoodleNoteData noodleData = (NoodleNoteData)NoodleObjectDatas[noteData];

            Quaternion? cutQuaternion = noodleData.CutQuaternion;

            NoteJump noteJump = _noteJumpAccessor(ref ____noteMovement);
            NoteFloorMovement floorMovement = _noteFloorMovementAccessor(ref ____noteMovement);

            if (cutQuaternion.HasValue)
            {
                Quaternion quatVal = cutQuaternion.Value;
                _endRotationAccessor(ref noteJump) = quatVal;
                Vector3 vector = quatVal.eulerAngles;
                vector += _randomRotationsAccessor(ref noteJump)[_randomRotationIdxAccessor(ref noteJump)] * 20;
                Quaternion midrotation = Quaternion.Euler(vector);
                _middleRotationAccessor(ref noteJump) = midrotation;
            }

            Quaternion? worldRotationQuaternion = noodleData.WorldRotationQuaternion;
            Quaternion? localRotationQuaternion = noodleData.LocalRotationQuaternion;

            Transform transform = __instance.transform;

            Quaternion localRotation = _quaternionIdentity;
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
                    _worldRotationJumpAccessor(ref noteJump) = quatVal;
                    _inverseWorldRotationJumpAccessor(ref noteJump) = inverseWorldRotation;
                    _worldRotationFloorAccessor(ref floorMovement) = quatVal;
                    _inverseWorldRotationFloorAccessor(ref floorMovement) = inverseWorldRotation;

                    quatVal *= localRotation;

                    transform.localRotation = quatVal;
                }
                else
                {
                    transform.localRotation *= localRotation;
                }
            }

            transform.localScale = Vector3.one; // This is a fix for animation due to notes being recycled

            Track track = noodleData.Track;
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

            noodleData.MoveStartPos = moveStartPos;
            noodleData.MoveEndPos = moveEndPos;
            noodleData.JumpEndPos = jumpEndPos;
            noodleData.WorldRotation = __instance.worldRotation;
            noodleData.LocalRotation = localRotation;
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
                    instructionList.Insert(i - 1, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(NoteController), "_noteData")));
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

            if (NoodleObjectDatas.TryGetValue(noteData, out NoodleObjectData noodleObjectData))
            {
                NoodleNoteData noodleData = (NoodleNoteData)noodleObjectData;
                float? flipYSide = noodleData.FlipYSideInternal;
                if (flipYSide.HasValue)
                {
                    output = flipYSide.Value;
                }
            }

            return output;
        }
    }

    [NoodlePatch(typeof(NoteController))]
    [NoodlePatch("ManualUpdate")]
    internal static class NoteControllerUpdate
    {
        internal static readonly FieldAccessor<NoteFloorMovement, Vector3>.Accessor _floorEndPosAccessor = FieldAccessor<NoteFloorMovement, Vector3>.GetAccessor("_endPos");
        private static readonly FieldAccessor<NoteFloorMovement, Vector3>.Accessor _floorStartPosAccessor = FieldAccessor<NoteFloorMovement, Vector3>.GetAccessor("_startPos");
        private static readonly FieldAccessor<NoteJump, Vector3>.Accessor _jumpStartPosAccessor = FieldAccessor<NoteJump, Vector3>.GetAccessor("_startPos");
        private static readonly FieldAccessor<NoteJump, Vector3>.Accessor _jumpEndPosAccessor = FieldAccessor<NoteJump, Vector3>.GetAccessor("_endPos");

        private static readonly FieldAccessor<NoteJump, IAudioTimeSource>.Accessor _audioTimeSyncControllerAccessor = FieldAccessor<NoteJump, IAudioTimeSource>.GetAccessor("_audioTimeSyncController");
        private static readonly FieldAccessor<NoteJump, float>.Accessor _jumpDurationAccessor = FieldAccessor<NoteJump, float>.GetAccessor("_jumpDuration");

        private static readonly FieldAccessor<BaseNoteVisuals, CutoutAnimateEffect>.Accessor _noteCutoutAnimateEffectAccessor = FieldAccessor<BaseNoteVisuals, CutoutAnimateEffect>.GetAccessor("_cutoutAnimateEffect");
        private static readonly FieldAccessor<CutoutAnimateEffect, CutoutEffect[]>.Accessor _cutoutEffectAccessor = FieldAccessor<CutoutAnimateEffect, CutoutEffect[]>.GetAccessor("_cuttoutEffects");

        private static readonly FieldAccessor<GameNoteController, BoxCuttableBySaber[]>.Accessor _gameNoteBigCuttableAccessor = FieldAccessor<GameNoteController, BoxCuttableBySaber[]>.GetAccessor("_bigCuttableBySaberList");
        private static readonly FieldAccessor<GameNoteController, BoxCuttableBySaber[]>.Accessor _gameNoteSmallCuttableAccessor = FieldAccessor<GameNoteController, BoxCuttableBySaber[]>.GetAccessor("_smallCuttableBySaberList");
        private static readonly FieldAccessor<BombNoteController, CuttableBySaber>.Accessor _bombNoteCuttableAccessor = FieldAccessor<BombNoteController, CuttableBySaber>.GetAccessor("_cuttableBySaber");

        private static readonly Dictionary<Type, MethodInfo> _setArrowTransparencyMethods = new Dictionary<Type, MethodInfo>();

        internal static NoodleObjectData NoodleData { get; private set; }

        private static void Prefix(NoteController __instance, NoteData ____noteData, NoteMovement ____noteMovement)
        {
            if (__instance is MultiplayerConnectedPlayerNoteController)
            {
                NoodleData = null;
                return;
            }

            NoodleData = NoodleObjectDatas[____noteData];
            NoodleNoteData noodleData = (NoodleNoteData)NoodleData;

            Track track = noodleData.Track;
            NoodleObjectData.AnimationObjectData animationObject = noodleData.AnimationObject;
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
                    Vector3 moveStartPos = noodleData.MoveStartPos;
                    Vector3 moveEndPos = noodleData.MoveEndPos;
                    Vector3 jumpEndPos = noodleData.JumpEndPos;

                    Vector3 offset = positionOffset.Value;
                    _floorStartPosAccessor(ref floorMovement) = moveStartPos + offset;
                    _floorEndPosAccessor(ref floorMovement) = moveEndPos + offset;
                    _jumpStartPosAccessor(ref noteJump) = moveEndPos + offset;
                    _jumpEndPosAccessor(ref noteJump) = jumpEndPos + offset;
                }

                Transform transform = __instance.transform;

                if (rotationOffset.HasValue || localRotationOffset.HasValue)
                {
                    Quaternion worldRotation = noodleData.WorldRotation;
                    Quaternion localRotation = noodleData.LocalRotation;

                    Quaternion worldRotationQuatnerion = worldRotation;
                    if (rotationOffset.HasValue)
                    {
                        worldRotationQuatnerion *= rotationOffset.Value;
                        Quaternion inverseWorldRotation = Quaternion.Inverse(worldRotationQuatnerion);
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
                    CutoutEffect cutoutEffect = noodleData.CutoutEffect;
                    if (cutoutEffect == null)
                    {
                        BaseNoteVisuals baseNoteVisuals = __instance.gameObject.GetComponent<BaseNoteVisuals>();
                        CutoutAnimateEffect cutoutAnimateEffect = _noteCutoutAnimateEffectAccessor(ref baseNoteVisuals);
                        CutoutEffect[] cutoutEffects = _cutoutEffectAccessor(ref cutoutAnimateEffect);
                        cutoutEffect = cutoutEffects.First(n => n.name != "NoteArrow"); // 1.11 NoteArrow has been added to the CutoutAnimateEffect and we don't want that
                        noodleData.CutoutEffect = cutoutEffect;
                    }

                    cutoutEffect.SetCutout(1 - dissolve.Value);
                }

                if (dissolveArrow.HasValue && __instance.noteData.colorType != ColorType.None)
                {
                    MonoBehaviour disappearingArrowController = noodleData.DisappearingArrowController;
                    if (disappearingArrowController == null)
                    {
                        disappearingArrowController = __instance.gameObject.GetComponent<DisappearingArrowControllerBase<GameNoteController>>();
                        if (disappearingArrowController == null)
                        {
                            disappearingArrowController = __instance.gameObject.GetComponent<DisappearingArrowControllerBase<MultiplayerConnectedPlayerGameNoteController>>();
                        }

                        noodleData.DisappearingArrowController = disappearingArrowController;
                        noodleData.DisappearingArrowMethod = GetSetArrowTransparency(disappearingArrowController.GetType());
                    }

                    // gross nasty reflection
                    noodleData.DisappearingArrowMethod.Invoke(disappearingArrowController, new object[] { dissolveArrow.Value });
                }

                if (cuttable.HasValue)
                {
                    bool enabled = cuttable.Value >= 1;

                    switch (__instance)
                    {
                        case GameNoteController gameNoteController:
                            BoxCuttableBySaber[] bigCuttableBySaberList = _gameNoteBigCuttableAccessor(ref gameNoteController);
                            foreach (BoxCuttableBySaber bigCuttableBySaber in bigCuttableBySaberList)
                            {
                                if (bigCuttableBySaber.canBeCut != enabled)
                                {
                                    bigCuttableBySaber.canBeCut = enabled;
                                }
                            }

                            BoxCuttableBySaber[] smallCuttableBySaberList = _gameNoteSmallCuttableAccessor(ref gameNoteController);
                            foreach (BoxCuttableBySaber smallCuttableBySaber in smallCuttableBySaberList)
                            {
                                if (smallCuttableBySaber.canBeCut != enabled)
                                {
                                    smallCuttableBySaber.canBeCut = enabled;
                                }
                            }

                            break;

                        case BombNoteController bombNoteController:
                            CuttableBySaber boxCuttableBySaber = _bombNoteCuttableAccessor(ref bombNoteController);
                            if (boxCuttableBySaber.canBeCut != enabled)
                            {
                                boxCuttableBySaber.canBeCut = enabled;
                            }

                            break;
                    }
                }
            }
        }

        private static MethodInfo GetSetArrowTransparency(Type type)
        {
            if (_setArrowTransparencyMethods.TryGetValue(type, out MethodInfo value))
            {
                return value;
            }

            Type baseType = type.BaseType;
            ////NoodleLogger.IPAlogger.Debug($"Base type is {baseType.Name}<{string.Join(", ", baseType.GenericTypeArguments.Select(t => t.Name))}>");
            MethodInfo method = baseType.GetMethod("SetArrowTransparency", BindingFlags.NonPublic | BindingFlags.Instance);
            _setArrowTransparencyMethods[type] = method;
            return method;
        }
    }
}
