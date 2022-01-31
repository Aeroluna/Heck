using System.Collections.Generic;
using Heck;
using Heck.Animation;
using IPA.Utilities;
using NoodleExtensions.Animation;
using NoodleExtensions.Managers;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;
using static NoodleExtensions.Extras.NoteAccessors;

namespace NoodleExtensions.HarmonyPatches.Objects
{
    internal class NoteUpdateNoodlifier : IAffinity
    {
        private static readonly FieldAccessor<NoteFloorMovement, Vector3>.Accessor _floorStartPosAccessor = FieldAccessor<NoteFloorMovement, Vector3>.GetAccessor("_startPos");
        private static readonly FieldAccessor<NoteJump, Vector3>.Accessor _jumpStartPosAccessor = FieldAccessor<NoteJump, Vector3>.GetAccessor("_startPos");
        private static readonly FieldAccessor<NoteJump, Vector3>.Accessor _jumpEndPosAccessor = FieldAccessor<NoteJump, Vector3>.GetAccessor("_endPos");

        private static readonly FieldAccessor<NoteJump, IAudioTimeSource>.Accessor _audioTimeSyncControllerAccessor = FieldAccessor<NoteJump, IAudioTimeSource>.GetAccessor("_audioTimeSyncController");
        private static readonly FieldAccessor<NoteJump, float>.Accessor _jumpDurationAccessor = FieldAccessor<NoteJump, float>.GetAccessor("_jumpDuration");

        private static readonly FieldAccessor<GameNoteController, BoxCuttableBySaber[]>.Accessor _gameNoteBigCuttableAccessor = FieldAccessor<GameNoteController, BoxCuttableBySaber[]>.GetAccessor("_bigCuttableBySaberList");
        private static readonly FieldAccessor<GameNoteController, BoxCuttableBySaber[]>.Accessor _gameNoteSmallCuttableAccessor = FieldAccessor<GameNoteController, BoxCuttableBySaber[]>.GetAccessor("_smallCuttableBySaberList");
        private static readonly FieldAccessor<BombNoteController, CuttableBySaber>.Accessor _bombNoteCuttableAccessor = FieldAccessor<BombNoteController, CuttableBySaber>.GetAccessor("_cuttableBySaber");

        private readonly CustomData _customData;
        private readonly AnimationHelper _animationHelper;
        private readonly CutoutManager _cutoutManager;
        private readonly LazyInject<NoteJumpNoodlifier> _noteJumpNoodlifier;

        private NoteUpdateNoodlifier(
            [Inject(Id = NoodleController.ID)] CustomData customData,
            AnimationHelper animationHelper,
            CutoutManager cutoutManager,
            LazyInject<NoteJumpNoodlifier> noteJumpNoodlifier)
        {
            _customData = customData;
            _animationHelper = animationHelper;
            _cutoutManager = cutoutManager;
            _noteJumpNoodlifier = noteJumpNoodlifier;
        }

        internal NoodleNoteData? NoodleData { get; private set; }

        [AffinityPrefix]
        [AffinityPatch(typeof(NoteController), nameof(NoteController.ManualUpdate))]
        private void Prefix(NoteController __instance, NoteData ____noteData, NoteMovement ____noteMovement)
        {
            if (!_customData.Resolve(____noteData, out NoodleNoteData? noodleData))
            {
                NoodleData = null;
                return;
            }

            NoodleData = noodleData;

            List<Track>? tracks = noodleData.Track;
            NoodleObjectData.AnimationObjectData? animationObject = noodleData.AnimationObject;
            if (tracks == null && animationObject == null)
            {
                return;
            }

            NoteJump noteJump = NoteJumpAccessor(ref ____noteMovement);
            NoteFloorMovement floorMovement = NoteFloorMovementAccessor(ref ____noteMovement);

            // idk i just copied base game time
            float jumpDuration = _jumpDurationAccessor(ref noteJump);
            float elapsedTime = _audioTimeSyncControllerAccessor(ref noteJump).songTime - (____noteData.time - (jumpDuration * 0.5f));
            elapsedTime = _noteJumpNoodlifier.Value.NoteJumpTimeAdjust(elapsedTime, jumpDuration);
            float normalTime = elapsedTime / jumpDuration;

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
                Vector3 moveStartPos = noodleData.MoveStartPos;
                Vector3 moveEndPos = noodleData.MoveEndPos;
                Vector3 jumpEndPos = noodleData.JumpEndPos;

                Vector3 offset = positionOffset.Value;
                _floorStartPosAccessor(ref floorMovement) = moveStartPos + offset;
                FloorEndPosAccessor(ref floorMovement) = moveEndPos + offset;
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
                    WorldRotationJumpAccessor(ref noteJump) = worldRotationQuatnerion;
                    InverseWorldRotationJumpAccessor(ref noteJump) = inverseWorldRotation;
                    WorldRotationFloorAccessor(ref floorMovement) = worldRotationQuatnerion;
                    InverseWorldRotationFloorAccessor(ref floorMovement) = inverseWorldRotation;
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
                if (_cutoutManager.NoteCutoutEffects.TryGetValue(__instance, out CutoutEffectWrapper cutoutEffect))
                {
                    cutoutEffect.SetCutout(dissolve.Value);
                }
            }

            if (dissolveArrow.HasValue && __instance.noteData.colorType != ColorType.None)
            {
                if (_cutoutManager.NoteDisappearingArrowWrappers.TryGetValue(__instance, out DisappearingArrowWrapper disappearingArrowWrapper))
                {
                    disappearingArrowWrapper.SetCutout(dissolveArrow.Value);
                }
            }

            if (!cuttable.HasValue)
            {
                return;
            }

            bool enabled = cuttable.Value >= 1;

            switch (__instance)
            {
                case GameNoteController gameNoteController:
                    BoxCuttableBySaber[] bigCuttableBySaberList = _gameNoteBigCuttableAccessor(ref gameNoteController);
                    foreach (BoxCuttableBySaber bigCuttableBySaber in bigCuttableBySaberList)
                    {
                        bigCuttableBySaber.canBeCut = enabled;
                    }

                    BoxCuttableBySaber[] smallCuttableBySaberList = _gameNoteSmallCuttableAccessor(ref gameNoteController);
                    foreach (BoxCuttableBySaber smallCuttableBySaber in smallCuttableBySaberList)
                    {
                        smallCuttableBySaber.canBeCut = enabled;
                    }

                    break;

                case BombNoteController bombNoteController:
                    CuttableBySaber boxCuttableBySaber = _bombNoteCuttableAccessor(ref bombNoteController);
                    boxCuttableBySaber.canBeCut = enabled;

                    break;
            }
        }
    }
}
