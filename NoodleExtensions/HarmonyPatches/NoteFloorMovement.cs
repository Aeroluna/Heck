namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;
    using NoodleExtensions.Animation;
    using UnityEngine;

    [HeckPatch(typeof(NoteFloorMovement))]
    [HeckPatch("ManualUpdate")]
    internal static class NoteFloorMovementManualUpdate
    {
        private static readonly FieldInfo _localPosition = AccessTools.Field(typeof(NoteFloorMovement), "_localPosition");

        private static readonly MethodInfo _definiteNoteFloorMovement = AccessTools.Method(typeof(NoteFloorMovementManualUpdate), nameof(DefiniteNoteFloorMovement));

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
            if (noodleData != null)
            {
                AnimationHelper.GetDefinitePositionOffset(noodleData.AnimationObject, noodleData.Track, 0, out Vector3? position);
                if (position.HasValue)
                {
                    Vector3 noteOffset = noodleData.NoteOffset;
                    Vector3 endPos = NoteControllerUpdate._floorEndPosAccessor(ref noteFloorMovement);
                    return original + (position.Value + noteOffset - endPos);
                }
            }

            return original;
        }
    }

    [HeckPatch(typeof(NoteFloorMovement))]
    [HeckPatch("SetToStart")]
    internal static class NoteFloorMovementSetToStart
    {
        private static void Postfix(Transform ____rotatedObject)
        {
            NoodleNoteData? noodleData = (NoodleNoteData?)NoteControllerUpdate.NoodleData;
            if (noodleData != null && noodleData.DisableLook)
            {
                ____rotatedObject.localRotation = Quaternion.Euler(0, 0, noodleData.EndRotation);
            }
        }
    }
}
