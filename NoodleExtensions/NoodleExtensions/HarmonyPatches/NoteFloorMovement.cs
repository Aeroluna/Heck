namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using NoodleExtensions.Animation;
    using UnityEngine;

    [NoodlePatch(typeof(NoteFloorMovement))]
    [NoodlePatch("ManualUpdate")]
    internal static class NoteFloorMovementManualUpdate
    {
        private static readonly MethodInfo _definiteNoteFloorMovement = SymbolExtensions.GetMethodInfo(() => DefiniteNoteFloorMovement(Vector3.zero, null));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundFinalPosition = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundFinalPosition &&
                    instructionList[i].opcode == OpCodes.Stfld &&
                    ((FieldInfo)instructionList[i].operand).Name == "_localPosition")
                {
                    foundFinalPosition = true;
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, _definiteNoteFloorMovement));
                }
            }

            if (!foundFinalPosition)
            {
                NoodleLogger.Log("Failed to find _localPosition stfld!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static Vector3 DefiniteNoteFloorMovement(Vector3 original, NoteFloorMovement noteFloorMovement)
        {
            NoodleObjectData noodleData = NoteControllerUpdate.NoodleData;
            AnimationHelper.GetDefinitePositionOffset(noodleData.AnimationObject, noodleData.Track, 0, out Vector3? position);
            if (position.HasValue)
            {
                Vector3 noteOffset = noodleData.NoteOffset;
                Vector3 endPos = NoteControllerUpdate._floorEndPosAccessor(ref noteFloorMovement);
                return original + (position.Value + noteOffset - endPos);
            }
            else
            {
                return original;
            }
        }
    }
}
