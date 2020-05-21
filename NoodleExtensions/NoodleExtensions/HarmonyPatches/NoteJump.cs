using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteJump))]
    [HarmonyPatch("ManualUpdate")]
    internal class NoteJumpManualUpdate
    {
        private static readonly FieldInfo _worldRotationField = AccessTools.Field(typeof(NoteJump), "_worldRotation");
        private static readonly FieldInfo _inverseWorldRotationField = AccessTools.Field(typeof(NoteJump), "_inverseWorldRotation");
        private static readonly FieldInfo _localPositionField = AccessTools.Field(typeof(NoteJump), "_localPosition");

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundPosition = false;
            bool foundTime = false;
            bool foundFinalPosition = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[i].opcode == OpCodes.Ldflda &&
                    (((FieldInfo)instructionList[i].operand).Name == "_startPos" ||
                    ((FieldInfo)instructionList[i].operand).Name == "_endPos"))
                {
                    foundPosition = true;
                    MethodInfo compositeMethod = null;
                    switch (((FieldInfo)instructionList[i + 1].operand).Name)
                    {
                        case "x":
                            compositeMethod = ObjectAnimationHelper._addCompositeX;
                            break;

                        case "y":
                            compositeMethod = ObjectAnimationHelper._addCompositeY;
                            break;

                        case "z":
                            compositeMethod = ObjectAnimationHelper._addCompositeZ;
                            break;
                    }
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Call, compositeMethod));
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
                    instructionList[i].opcode == OpCodes.Stind_R4)
                {
                    foundFinalPosition = true;
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 3, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 4, new CodeInstruction(OpCodes.Ldfld, _localPositionField));
                    instructionList.Insert(i + 5, new CodeInstruction(OpCodes.Call, ObjectAnimationHelper._addActivePosition));
                    instructionList.Insert(i + 6, new CodeInstruction(OpCodes.Stfld, _localPositionField));
                }
            }
            if (!foundPosition) Logger.Log("Failed to find _startPos or _endPos ldfld, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            if (!foundTime) Logger.Log("Failed to find stloc.0, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            if (!foundFinalPosition) Logger.Log("Failed to find stind.r4, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
        }
    }
}