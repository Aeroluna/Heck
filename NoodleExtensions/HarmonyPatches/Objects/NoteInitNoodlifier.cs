using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using Heck.Animation;
using IPA.Utilities;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;
using static NoodleExtensions.Extras.NoteAccessors;

namespace NoodleExtensions.HarmonyPatches.Objects
{
    internal class NoteInitNoodlifier : IAffinity, IDisposable
    {
        private static readonly FieldAccessor<NoteJump, Quaternion>.Accessor _endRotationAccessor = FieldAccessor<NoteJump, Quaternion>.GetAccessor("_endRotation");
        private static readonly FieldAccessor<NoteJump, Quaternion>.Accessor _middleRotationAccessor = FieldAccessor<NoteJump, Quaternion>.GetAccessor("_middleRotation");
        private static readonly FieldAccessor<NoteJump, Vector3[]>.Accessor _randomRotationsAccessor = FieldAccessor<NoteJump, Vector3[]>.GetAccessor("_randomRotations");
        private static readonly FieldAccessor<NoteJump, int>.Accessor _randomRotationIdxAccessor = FieldAccessor<NoteJump, int>.GetAccessor("_randomRotationIdx");

        private static readonly FieldInfo _noteDataField = AccessTools.Field(typeof(NoteController), "_noteData");
        private static readonly MethodInfo _flipYSideGetter = AccessTools.PropertyGetter(typeof(NoteData), nameof(NoteData.flipYSide));

        private readonly CodeInstruction _flipYSide;
        private readonly CustomData _customData;

        private NoteInitNoodlifier([Inject(Id = NoodleController.ID)] CustomData customData)
        {
            _customData = customData;
            _flipYSide = InstanceTranspilers.EmitInstanceDelegate<Func<NoteData, float, float>>(GetFlipYSide);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_flipYSide);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(NoteController), "Init")]
        private void Postfix(NoteController __instance, NoteData noteData, NoteMovement ____noteMovement, Vector3 moveStartPos, Vector3 moveEndPos, Vector3 jumpEndPos, float endRotation)
        {
            if (!_customData.Resolve(noteData, out NoodleNoteData? noodleData))
            {
                return;
            }

            Quaternion? cutQuaternion = noodleData.CutQuaternion;

            NoteJump noteJump = NoteJumpAccessor(ref ____noteMovement);
            NoteFloorMovement floorMovement = NoteFloorMovementAccessor(ref ____noteMovement);

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
                    WorldRotationJumpAccessor(ref noteJump) = quatVal;
                    InverseWorldRotationJumpAccessor(ref noteJump) = inverseWorldRotation;
                    WorldRotationFloorAccessor(ref floorMovement) = quatVal;
                    InverseWorldRotationFloorAccessor(ref floorMovement) = inverseWorldRotation;

                    quatVal *= localRotation;

                    transform.localRotation = quatVal;
                }
                else
                {
                    transform.localRotation *= localRotation;
                }
            }

            transform.localScale = Vector3.one; // This is a fix for animation due to notes being recycled

            IEnumerable<Track>? tracks = noodleData.Track;
            if (tracks != null)
            {
                foreach (Track track in tracks)
                {
                    // add to gameobjects
                    track.AddGameObject(__instance.gameObject);
                }
            }

            // how fucking long has _zOffset existed???!??
            float zOffset = ZOffsetAccessor(ref ____noteMovement);
            moveStartPos.z += zOffset;
            moveEndPos.z += zOffset;
            jumpEndPos.z += zOffset;

            noodleData.EndRotation = endRotation;
            noodleData.MoveStartPos = moveStartPos;
            noodleData.MoveEndPos = moveEndPos;
            noodleData.JumpEndPos = jumpEndPos;
            noodleData.WorldRotation = __instance.worldRotation;
            noodleData.LocalRotation = localRotation;
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(NoteController), "Init")]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _flipYSideGetter))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _noteDataField))
                .Advance(1)
                .Insert(_flipYSide)
                .InstructionEnumeration();
        }

        private float GetFlipYSide(NoteData noteData, float @default)
        {
            _customData.Resolve(noteData, out NoodleNoteData? noodleData);
            return noodleData?.FlipYSideInternal ?? @default;
        }
    }
}
