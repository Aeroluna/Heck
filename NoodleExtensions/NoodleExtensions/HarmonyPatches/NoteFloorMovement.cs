using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteFloorMovement))]
    [HarmonyPatch("ManualUpdate")]
    internal class NoteFloorMovementManualUpdate
    {
        private static readonly FieldInfo _worldRotationField = AccessTools.Field(typeof(NoteFloorMovement), "_worldRotation");
        private static readonly FieldInfo _inverseWorldRotationField = AccessTools.Field(typeof(NoteFloorMovement), "_inverseWorldRotation");

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundPosition = false;
            bool foundTime = false;
            bool foundFinalPosition = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[i].opcode == OpCodes.Ldfld &&
                    (((FieldInfo)instructionList[i].operand).Name == "_startPos" ||
                    ((FieldInfo)instructionList[i].operand).Name == "_endPos"))
                {
                    foundPosition = true;
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, ObjectAnimationHelper._addCompositePos));
                }
                if (!foundTime &&
                    instructionList[i].opcode == OpCodes.Stloc_0)
                {
                    foundTime = true;
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Ldflda, _worldRotationField));
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 3, new CodeInstruction(OpCodes.Ldflda, _inverseWorldRotationField));
                    instructionList.Insert(i + 4, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 5, new CodeInstruction(OpCodes.Call, ObjectAnimationHelper._handleNoteAnimation));
                }
                if (!foundFinalPosition &&
                    instructionList[i].opcode == OpCodes.Stfld &&
                    ((FieldInfo)instructionList[i].operand).Name == "_localPosition")
                {
                    foundFinalPosition = true;
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Call, ObjectAnimationHelper._addActivePosition));
                }
            }
            if (!foundPosition) Logger.Log("Failed to find _startPos or _endPos ldfld, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            if (!foundTime) Logger.Log("Failed to find stloc.0, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            if (!foundFinalPosition) Logger.Log("Failed to find _localPosition stfld, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
        }
    }
}