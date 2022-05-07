using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using NoodleExtensions.Animation;
using SiraUtil.Affinity;
using UnityEngine;
using static NoodleExtensions.Extras.NoteAccessors;

namespace NoodleExtensions.HarmonyPatches.Objects
{
    internal class NoteFloorMovementNoodlifier : IAffinity, IDisposable
    {
        private static readonly FieldInfo _localPosition = AccessTools.Field(typeof(NoteFloorMovement), "_localPosition");

        private readonly CodeInstruction _definiteNoteFloorMovement;
        private readonly NoteUpdateNoodlifier _noteUpdateNoodlifier;
        private readonly AnimationHelper _animationHelper;

        private NoteFloorMovementNoodlifier(NoteUpdateNoodlifier noteUpdateNoodlifier, AnimationHelper animationHelper)
        {
            _noteUpdateNoodlifier = noteUpdateNoodlifier;
            _animationHelper = animationHelper;
            _definiteNoteFloorMovement = InstanceTranspilers.EmitInstanceDelegate<Func<Vector3, NoteFloorMovement, Vector3>>(DefiniteNoteFloorMovement);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_definiteNoteFloorMovement);
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(NoteFloorMovement), nameof(NoteFloorMovement.ManualUpdate))]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Stfld, _localPosition))
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    _definiteNoteFloorMovement)
                .InstructionEnumeration();
        }

        private Vector3 DefiniteNoteFloorMovement(Vector3 original, NoteFloorMovement noteFloorMovement)
        {
            NoodleObjectData? noodleData = _noteUpdateNoodlifier.NoodleData;
            if (noodleData == null)
            {
                return original;
            }

            _animationHelper.GetDefinitePositionOffset(noodleData.AnimationObject, noodleData.Track, 0, out Vector3? position);
            if (!position.HasValue)
            {
                return original;
            }

            Vector3 noteOffset = noodleData.InternalNoteOffset;
            Vector3 endPos = FloorEndPosAccessor(ref noteFloorMovement);
            return original + (position.Value + noteOffset - endPos);
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(NoteFloorMovement), nameof(NoteFloorMovement.SetToStart))]
        private void NoteFloorMovementSetToStart(Transform ____rotatedObject)
        {
            NoodleNoteData? noodleData = _noteUpdateNoodlifier.NoodleData;
            if (noodleData is { DisableLook: true })
            {
                ____rotatedObject.localRotation = Quaternion.Euler(0, 0, noodleData.InternalEndRotation);
            }
        }
    }
}
