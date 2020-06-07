using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using IPA.Utilities;
using NoodleExtensions.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [NoodlePatch(typeof(NoteController))]
    [NoodlePatch("Init")]
    internal class NoteControllerInit
    {
        internal static readonly FieldAccessor<NoteMovement, NoteJump>.Accessor _noteJumpAccessor = FieldAccessor<NoteMovement, NoteJump>.GetAccessor("_jump");
        internal static readonly FieldAccessor<NoteMovement, NoteFloorMovement>.Accessor _noteFloorMovementAccessor = FieldAccessor<NoteMovement, NoteFloorMovement>.GetAccessor("_floorMovement");

        private static readonly FieldAccessor<NoteJump, Quaternion>.Accessor _endRotationAccessor = FieldAccessor<NoteJump, Quaternion>.GetAccessor("_endRotation");
        private static readonly FieldAccessor<NoteJump, Quaternion>.Accessor _middleRotationAccessor = FieldAccessor<NoteJump, Quaternion>.GetAccessor("_middleRotation");
        private static readonly FieldAccessor<NoteJump, Vector3[]>.Accessor _randomRotationsAccessor = FieldAccessor<NoteJump, Vector3[]>.GetAccessor("_randomRotations");
        private static readonly FieldAccessor<NoteJump, int>.Accessor _randomRotationIdxAccessor = FieldAccessor<NoteJump, int>.GetAccessor("_randomRotationIdx");
        internal static readonly FieldAccessor<NoteJump, Quaternion>.Accessor _worldRotationJumpAccessor = FieldAccessor<NoteJump, Quaternion>.GetAccessor("_worldRotation");
        internal static readonly FieldAccessor<NoteJump, Quaternion>.Accessor _inverseWorldRotationJumpAccessor = FieldAccessor<NoteJump, Quaternion>.GetAccessor("_inverseWorldRotation");

        internal static readonly FieldAccessor<NoteFloorMovement, Quaternion>.Accessor _worldRotationFloorAccessor = FieldAccessor<NoteFloorMovement, Quaternion>.GetAccessor("_worldRotation");
        internal static readonly FieldAccessor<NoteFloorMovement, Quaternion>.Accessor _inverseWorldRotationFloorAccessor = FieldAccessor<NoteFloorMovement, Quaternion>.GetAccessor("_inverseWorldRotation");

        private static void Postfix(NoteController __instance, NoteData noteData, NoteMovement ____noteMovement, Vector3 moveStartPos, Vector3 moveEndPos, Vector3 jumpEndPos)
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
                Vector3 worldRotation = Vector3.zero;
                if (rotation != null)
                {
                    if (rotation is List<object> list)
                    {
                        IEnumerable<float> _rot = (list)?.Select(n => Convert.ToSingle(n));
                        worldRotation = new Vector3(_rot.ElementAt(0), _rot.ElementAt(1), _rot.ElementAt(2));
                    }
                    else worldRotation = new Vector3(0, (float)rotation, 0);
                    Quaternion worldRotationQuatnerion = Quaternion.Euler(worldRotation);
                    Quaternion inverseWorldRotation = Quaternion.Inverse(worldRotationQuatnerion);
                    _worldRotationJumpAccessor(ref noteJump) = worldRotationQuatnerion;
                    _inverseWorldRotationJumpAccessor(ref noteJump) = inverseWorldRotation;
                    _worldRotationFloorAccessor(ref floorMovement) = worldRotationQuatnerion;
                    _inverseWorldRotationFloorAccessor(ref floorMovement) = inverseWorldRotation;
                    __instance.transform.rotation = worldRotationQuatnerion;
                }
                //TODO: fix local rot to only modify transform once
                IEnumerable<float> localRotRaw = ((List<object>)Trees.at(dynData, LOCALROTATION))?.Select(n => Convert.ToSingle(n));
                Vector3 localRotation = Vector3.zero;
                if (localRotRaw != null)
                {
                    localRotation = new Vector3(localRotRaw.ElementAt(0), localRotRaw.ElementAt(1), localRotRaw.ElementAt(2));
                    __instance.transform.Rotate(localRotation);
                }

                dynData.moveStartPos = moveStartPos;
                dynData.moveEndPos = moveEndPos;
                dynData.jumpEndPos = jumpEndPos;
                dynData.localRotation = localRotation;
                dynData.worldRotation = worldRotation;
            }
        }

        private static readonly MethodInfo _getFlipYSide = SymbolExtensions.GetMethodInfo(() => GetFlipYSide(null, 0));

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
            if (!foundFlipYSide) Logger.Log("Failed to find Get_flipYSide call, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
        }

        private static float GetFlipYSide(NoteData noteData, float @default)
        {
            float output = @default;
            if (noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;

                float? flipYSide = (float?)Trees.at(dynData, "flipYSide");
                if (flipYSide.HasValue) output = flipYSide.Value;
            }
            return output;
        }
    }

    [NoodlePatch(typeof(NoteController))]
    [NoodlePatch("Update")]
    internal class NoteControllerUpdate
    {
        private static readonly FieldAccessor<NoteFloorMovement, Vector3>.Accessor _floorStartPosAccessor = FieldAccessor<NoteFloorMovement, Vector3>.GetAccessor("_startPos");
        internal static readonly FieldAccessor<NoteFloorMovement, Vector3>.Accessor _floorEndPosAccessor = FieldAccessor<NoteFloorMovement, Vector3>.GetAccessor("_endPos");
        private static readonly FieldAccessor<NoteJump, Vector3>.Accessor _jumpStartPosAccessor = FieldAccessor<NoteJump, Vector3>.GetAccessor("_startPos");
        private static readonly FieldAccessor<NoteJump, Vector3>.Accessor _jumpEndPosAccessor = FieldAccessor<NoteJump, Vector3>.GetAccessor("_endPos");

        private static readonly FieldAccessor<NoteJump, AudioTimeSyncController>.Accessor _audioTimeSyncControllerAccessor = FieldAccessor<NoteJump, AudioTimeSyncController>.GetAccessor("_audioTimeSyncController");
        private static readonly FieldAccessor<NoteJump, float>.Accessor _jumpDurationAccessor = FieldAccessor<NoteJump, float>.GetAccessor("_jumpDuration");

        private static readonly FieldAccessor<BaseNoteVisuals, CutoutAnimateEffect>.Accessor _noteCutoutAnimateEffectAccessor = FieldAccessor<BaseNoteVisuals, CutoutAnimateEffect>.GetAccessor("_cutoutAnimateEffect");

        internal static CustomNoteData _customNoteData;

        private static void Prefix(NoteController __instance, NoteData ____noteData, NoteMovement ____noteMovement)
        {
            if (____noteData is CustomNoteData customData)
            {
                _customNoteData = customData;

                dynamic dynData = customData.customData;

                Track track = Trees.at(dynData, "track");
                if (track != null)
                {
                    NoteJump noteJump = NoteControllerInit._noteJumpAccessor(ref ____noteMovement);
                    NoteFloorMovement floorMovement = NoteControllerInit._noteFloorMovementAccessor(ref ____noteMovement);
                    Vector3 moveStartPos = Trees.at(dynData, "moveStartPos");
                    Vector3 moveEndPos = Trees.at(dynData, "moveEndPos");
                    Vector3 jumpEndPos = Trees.at(dynData, "jumpEndPos");
                    // TODO: allow local rotation to be null
                    Vector3 localRotation = Trees.at(dynData, "localRotation");
                    Vector3 worldRotation = Trees.at(dynData, "worldRotation");

                    // idk i just copied base game time
                    float jumpDuration = _jumpDurationAccessor(ref noteJump);
                    float elapsedTime = _audioTimeSyncControllerAccessor(ref noteJump).songTime - (____noteData.time - jumpDuration * 0.5f);
                    float normalTime = elapsedTime / jumpDuration;

                    dynamic animationObject = Trees.at(dynData, "_animation");
                    AnimationHelper.GetObjectOffset(animationObject, track, normalTime, out Vector3? positionOffset, out Vector3? rotationOffset, out Vector3? scaleOffset, out Vector3? localRotationOffset);

                    if (positionOffset.HasValue)
                    {
                        Vector3 offset = positionOffset.Value;
                        _floorStartPosAccessor(ref floorMovement) = moveStartPos + offset;
                        _floorEndPosAccessor(ref floorMovement) = moveEndPos + offset;
                        _jumpStartPosAccessor(ref noteJump) = moveEndPos + offset;
                        _jumpEndPosAccessor(ref noteJump) = jumpEndPos + offset;
                    }

                    Transform transform = __instance.transform;

                    if (rotationOffset.HasValue || localRotationOffset.HasValue)
                    {
                        Quaternion worldRotationQuatnerion;
                        if (rotationOffset.HasValue)
                        {
                            worldRotationQuatnerion = Quaternion.Euler(worldRotation + rotationOffset.Value);
                            Quaternion inverseWorldRotation = Quaternion.Inverse(worldRotationQuatnerion);
                            NoteControllerInit._worldRotationJumpAccessor(ref noteJump) = worldRotationQuatnerion;
                            NoteControllerInit._inverseWorldRotationJumpAccessor(ref noteJump) = inverseWorldRotation;
                            NoteControllerInit._worldRotationFloorAccessor(ref floorMovement) = worldRotationQuatnerion;
                            NoteControllerInit._inverseWorldRotationFloorAccessor(ref floorMovement) = inverseWorldRotation;
                        }
                        else
                        {
                            worldRotationQuatnerion = Quaternion.Euler(worldRotation);
                        }

                        Vector3? localRotationSum = AnimationHelper.SumVectorNullables(localRotation, localRotationOffset);

                        if (localRotationSum.HasValue) worldRotationQuatnerion *= Quaternion.Euler(localRotation + localRotationOffset.Value);

                        transform.rotation = worldRotationQuatnerion;
                    }

                    if (scaleOffset.HasValue) transform.localScale = scaleOffset.Value;

                    // TODO: integrate dissolve into path feature
                    // TODO: null all track defaults
                    CutoutAnimateEffect cutoutAnimateEffect = Trees.at(dynData, "cutoutAnimateEffect");
                    if (cutoutAnimateEffect == null)
                    {
                        BaseNoteVisuals baseNoteVisuals = __instance.gameObject.GetComponent<BaseNoteVisuals>();
                        cutoutAnimateEffect = _noteCutoutAnimateEffectAccessor(ref baseNoteVisuals);
                        dynData.cutoutAnimateEffect = cutoutAnimateEffect;
                    }
                    cutoutAnimateEffect.SetCutout(track.dissolve);

                    // null checked because bombs do not have a DisappearingArrowController
                    DisappearingArrowController disappearingArrowController = Trees.at(dynData, "disappearingArrowController");
                    if (disappearingArrowController == null)
                    {
                        disappearingArrowController = __instance.gameObject.GetComponent<DisappearingArrowController>();
                        dynData.disappearingArrowController = disappearingArrowController;
                    }
                    disappearingArrowController?.SetArrowTransparency(1 - track.dissolveArrow);
                }
            }
        }
    }
}