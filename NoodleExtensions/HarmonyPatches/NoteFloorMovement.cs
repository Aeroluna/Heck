using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using JetBrains.Annotations;
using NoodleExtensions.Animation;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches
{
    [HeckPatch(typeof(NoteFloorMovement))]
    [HeckPatch("ManualUpdate")]
    internal static class NoteFloorMovementManualUpdate
    {
        private static readonly FieldInfo _localPosition = AccessTools.Field(typeof(NoteFloorMovement), "_localPosition");

        private static readonly MethodInfo _definiteNoteFloorMovement = AccessTools.Method(typeof(NoteFloorMovementManualUpdate), nameof(DefiniteNoteFloorMovement));

        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Stfld, _localPosition))
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, _definiteNoteFloorMovement))
                .InstructionEnumeration();
        }

        private static Vector3 DefiniteNoteFloorMovement(Vector3 original, NoteFloorMovement noteFloorMovement)
        {
            NoodleObjectData? noodleData = NoteControllerUpdate.NoodleData;
            if (noodleData == null)
            {
                return original;
            }

            AnimationHelper.GetDefinitePositionOffset(noodleData.AnimationObject, noodleData.Track, 0, out Vector3? position);
            if (!position.HasValue)
            {
                return original;
            }

            Vector3 noteOffset = noodleData.NoteOffset;
            Vector3 endPos = NoteControllerUpdate._floorEndPosAccessor(ref noteFloorMovement);
            return original + (position.Value + noteOffset - endPos);
        }
    }

    [HeckPatch(typeof(NoteFloorMovement))]
    [HeckPatch("SetToStart")]
    internal static class NoteFloorMovementSetToStart
    {
        [UsedImplicitly]
        private static void Postfix(Transform ____rotatedObject)
        {
            NoodleNoteData? noodleData = (NoodleNoteData?)NoteControllerUpdate.NoodleData;
            if (noodleData is { DisableLook: true })
            {
                ____rotatedObject.localRotation = Quaternion.Euler(0, 0, noodleData.EndRotation);
            }
        }
    }
}
