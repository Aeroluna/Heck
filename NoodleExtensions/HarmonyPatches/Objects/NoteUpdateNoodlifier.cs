using System.Collections.Generic;
using Heck.Animation;
using Heck.Deserialize;
using NoodleExtensions.Animation;
using NoodleExtensions.Managers;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace NoodleExtensions.HarmonyPatches.Objects;

internal class NoteUpdateNoodlifier : IAffinity
{
    private readonly AnimationHelper _animationHelper;
    private readonly CutoutManager _cutoutManager;
    private readonly IAudioTimeSource _audioTimeSource;
    private readonly DeserializedData _deserializedData;

    private NoteUpdateNoodlifier(
        [Inject(Id = NoodleController.ID)] DeserializedData deserializedData,
        AnimationHelper animationHelper,
        CutoutManager cutoutManager,
        IAudioTimeSource audioTimeSource)
    {
        _deserializedData = deserializedData;
        _animationHelper = animationHelper;
        _cutoutManager = cutoutManager;
        _audioTimeSource = audioTimeSource;
    }

    internal NoodleBaseNoteData? NoodleData { get; private set; }

    [AffinityPrefix]
    [AffinityPatch(typeof(NoteController), nameof(NoteController.ManualUpdate))]
    private void Prefix(NoteController __instance, NoteData ____noteData, NoteMovement ____noteMovement)
    {
        if (!_deserializedData.Resolve(____noteData, out NoodleBaseNoteData? noodleData))
        {
            NoodleData = null;
            return;
        }

        NoodleData = noodleData;

        IReadOnlyList<Track>? tracks = noodleData.Track;
        NoodleObjectData.AnimationObjectData? animationObject = noodleData.AnimationObject;
        if (tracks == null && animationObject == null)
        {
            return;
        }

        NoteJump noteJump = ____noteMovement._jump;
        NoteFloorMovement floorMovement = ____noteMovement._floorMovement;
#if LATEST
        IVariableMovementDataProvider variableMovementDataProvider = ____noteMovement._variableMovementDataProvider;
#endif

        float? time = noodleData.GetTimeProperty();
        float normalTime;
        if (time.HasValue)
        {
            normalTime = time.Value;
        }
        else
        {
#if LATEST
            float jumpDuration = variableMovementDataProvider.jumpDuration;
#else
            float jumpDuration = noteJump._jumpDuration;
#endif
            float elapsedTime = _audioTimeSource.songTime - (____noteData.time - (jumpDuration * 0.5f));
            normalTime = elapsedTime / jumpDuration;
        }

        _animationHelper.GetObjectOffset(
            animationObject,
            tracks,
            normalTime,
            out Vector3? positionOffset,
            out Quaternion? rotationOffset,
            out Vector3? scaleOffset,
            out Quaternion? localRotationOffset,
            out float? dissolve,
            out float? dissolveArrow,
            out float? cuttable);

        if (positionOffset.HasValue)
        {
            Vector3 moveStartPos = noodleData.InternalStartPos;
            Vector3 moveEndPos = noodleData.InternalMidPos;
            Vector3 jumpEndPos = noodleData.InternalEndPos;

            Vector3 offset = positionOffset.Value;
#if LATEST
            floorMovement._moveStartOffset = moveStartPos + offset;
            floorMovement._moveEndOffset = moveEndPos + offset;
            noteJump._startOffset = moveEndPos + offset;
            noteJump._endOffset = jumpEndPos + offset;
            noteJump._startPos = variableMovementDataProvider.moveEndPosition + noteJump._startOffset;
            noteJump._endPos = variableMovementDataProvider.jumpEndPosition + noteJump._endOffset;
#else
            floorMovement._startPos = moveStartPos + offset;
            floorMovement._endPos = moveEndPos + offset;
            noteJump._startPos = moveEndPos + offset;
            noteJump._endPos = jumpEndPos + offset;
#endif
        }

        Transform transform = __instance.transform;

        if (rotationOffset.HasValue || localRotationOffset.HasValue)
        {
            Quaternion worldRotation = noodleData.InternalWorldRotation;
            Quaternion localRotation = noodleData.InternalLocalRotation;

            Quaternion worldRotationQuaternion = worldRotation;
            if (rotationOffset.HasValue)
            {
                worldRotationQuaternion *= rotationOffset.Value;
                Quaternion inverseWorldRotation = Quaternion.Inverse(worldRotationQuaternion);
                noteJump._worldRotation = worldRotationQuaternion;
                noteJump._inverseWorldRotation = inverseWorldRotation;
                floorMovement._worldRotation = worldRotationQuaternion;
                floorMovement._inverseWorldRotation = inverseWorldRotation;
            }

            worldRotationQuaternion *= localRotation;

            if (localRotationOffset.HasValue)
            {
                worldRotationQuaternion *= localRotationOffset.Value;
            }

            transform.localRotation = worldRotationQuaternion;
        }

        if (scaleOffset.HasValue)
        {
            transform.localScale = Vector3.Scale(noodleData.InternalScale, scaleOffset.Value);
        }

        if (dissolve.HasValue)
        {
            _cutoutManager.NoteCutoutEffects[__instance].SetCutout(dissolve.Value);
        }

        if (dissolveArrow.HasValue && __instance.noteData.colorType != ColorType.None)
        {
            _cutoutManager.NoteDisappearingArrowWrappers[__instance].SetCutout(dissolveArrow.Value);
        }

        if (!cuttable.HasValue)
        {
            return;
        }

        bool enabled = cuttable.Value >= 1;

        switch (__instance)
        {
            case GameNoteController gameNoteController:
                foreach (BoxCuttableBySaber bigCuttableBySaber in gameNoteController._bigCuttableBySaberList)
                {
                    bigCuttableBySaber.canBeCut = enabled;
                }

                foreach (BoxCuttableBySaber smallCuttableBySaber in gameNoteController._smallCuttableBySaberList)
                {
                    smallCuttableBySaber.canBeCut = enabled;
                }

                break;

            case BombNoteController bombNoteController:
                bombNoteController._cuttableBySaber.canBeCut = enabled;

                break;
        }
    }
}
