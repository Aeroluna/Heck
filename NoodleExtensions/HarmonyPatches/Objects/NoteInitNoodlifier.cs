using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using Heck.Animation;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace NoodleExtensions.HarmonyPatches.Objects
{
    internal class NoteInitNoodlifier : IAffinity, IDisposable
    {
        private static readonly FieldInfo _noteDataField = AccessTools.Field(typeof(NoteController), "_noteData");
        private static readonly MethodInfo _flipYSideGetter = AccessTools.PropertyGetter(typeof(NoteData), nameof(NoteData.flipYSide));

        private readonly CodeInstruction _flipYSide;
        private readonly DeserializedData _deserializedData;

        private NoteInitNoodlifier([Inject(Id = NoodleController.ID)] DeserializedData deserializedData)
        {
            _deserializedData = deserializedData;
            _flipYSide = InstanceTranspilers.EmitInstanceDelegate<Func<NoteData, float, float>>(GetFlipYSide);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_flipYSide);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(NoteController), "Init")]
        private void Postfix(
            NoteController __instance,
            NoteData noteData,
            NoteMovement ____noteMovement,
            Vector3 moveStartPos,
            Vector3 moveEndPos,
            Vector3 jumpEndPos,
            float jumpDuration,
            float jumpGravity,
            float endRotation,
            bool useRandomRotation)
        {
            if (!_deserializedData.Resolve(noteData, out NoodleBaseNoteData? noodleData))
            {
                return;
            }

            // how fucking long has _zOffset existed???!??
            float zOffset = ____noteMovement._zOffset;
            moveStartPos.z += zOffset;
            moveEndPos.z += zOffset;
            jumpEndPos.z += zOffset;

            NoteJump noteJump = ____noteMovement._jump;
            NoteFloorMovement floorMovement = ____noteMovement._floorMovement;

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
                    noteJump._worldRotation = quatVal;
                    noteJump._inverseWorldRotation = inverseWorldRotation;
                    floorMovement._worldRotation = quatVal;
                    floorMovement._inverseWorldRotation = inverseWorldRotation;

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

            noodleData.InternalEndRotation = endRotation;
            noodleData.InternalStartPos = moveStartPos;
            noodleData.InternalMidPos = moveEndPos;
            noodleData.InternalEndPos = jumpEndPos;
            noodleData.InternalWorldRotation = __instance.worldRotation;
            noodleData.InternalLocalRotation = localRotation;

            float num2 = jumpDuration * 0.5f;
            float startVerticalVelocity = jumpGravity * num2;
            float yOffset = (startVerticalVelocity * num2) - (jumpGravity * num2 * num2 * 0.5f);
            noodleData.InternalNoteOffset = new Vector3(jumpEndPos.x, moveEndPos.y + yOffset, 0);
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(NoteController), "Init")]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * -- this._noteMovement.Init(noteData.time, worldRotation, moveStartPos, moveEndPos, jumpEndPos, moveDuration, jumpDuration, jumpGravity, noteData.flipYSide, endRotation, rotateTowardsPlayer, useRandomRotation);
                 * ++ this._noteMovement.Init(noteData.time, worldRotation, moveStartPos, moveEndPos, jumpEndPos, moveDuration, jumpDuration, jumpGravity, GetFlipYSide(noteData, noteData.flipYSide), endRotation, rotateTowardsPlayer, useRandomRotation);
                 */
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
            _deserializedData.Resolve(noteData, out NoodleBaseNoteData? noodleData);
            return noodleData?.InternalFlipYSide ?? @default;
        }
    }
}
