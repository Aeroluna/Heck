using System.Collections.Generic;
using Heck.Animation;
using Heck.Deserialize;
using NoodleExtensions.Animation;
using NoodleExtensions.Managers;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace NoodleExtensions.HarmonyPatches.Objects
{
    internal class NoteUpdateNoodlifier : IAffinity
    {
        private readonly DeserializedData _deserializedData;
        private readonly AnimationHelper _animationHelper;
        private readonly CutoutManager _cutoutManager;
        private readonly AudioTimeSyncController _audioTimeSyncController;

        private NoteUpdateNoodlifier(
            [Inject(Id = NoodleController.ID)] DeserializedData deserializedData,
            AnimationHelper animationHelper,
            CutoutManager cutoutManager,
            AudioTimeSyncController audioTimeSyncController)
        {
            _deserializedData = deserializedData;
            _animationHelper = animationHelper;
            _cutoutManager = cutoutManager;
            _audioTimeSyncController = audioTimeSyncController;
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

            float? time = noodleData.GetTimeProperty();
            float normalTime;
            if (time.HasValue)
            {
                normalTime = time.Value;
            }
            else
            {
                float jumpDuration = noteJump._jumpDuration;
                float elapsedTime = _audioTimeSyncController.songTime - (____noteData.time - (jumpDuration * 0.5f));
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
                floorMovement._startPos = moveStartPos + offset;
                floorMovement._endPos = moveEndPos + offset;
                noteJump._startPos = moveEndPos + offset;
                noteJump._endPos = jumpEndPos + offset;
            }

            Transform transform = __instance.transform;

            if (rotationOffset.HasValue || localRotationOffset.HasValue)
            {
                Quaternion worldRotation = noodleData.InternalWorldRotation;
                Quaternion localRotation = noodleData.InternalLocalRotation;

                Quaternion worldRotationQuatnerion = worldRotation;
                if (rotationOffset.HasValue)
                {
                    worldRotationQuatnerion *= rotationOffset.Value;
                    Quaternion inverseWorldRotation = Quaternion.Inverse(worldRotationQuatnerion);
                    noteJump._worldRotation = worldRotationQuatnerion;
                    noteJump._inverseWorldRotation = inverseWorldRotation;
                    floorMovement._worldRotation = worldRotationQuatnerion;
                    floorMovement._inverseWorldRotation = inverseWorldRotation;
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
}
