using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteFloorMovement))]
    [HarmonyPatch("ManualUpdate")]
    internal class NoteFloorMovementManualUpdate
    {
        private static readonly FieldInfo WorldRotationField = AccessTools.Field(typeof(NoteFloorMovement), "_worldRotation");
        private static readonly FieldInfo InverseWorldRotationField = AccessTools.Field(typeof(NoteFloorMovement), "_inverseWorldRotation");
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
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, ObjectAnimationHelper.AddComposite));
                }
                if (!foundTime &&
                    instructionList[i].opcode == OpCodes.Stloc_0)
                {
                    foundTime = true;
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Ldflda, WorldRotationField));
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 3, new CodeInstruction(OpCodes.Ldflda, InverseWorldRotationField));
                    instructionList.Insert(i + 4, new CodeInstruction(OpCodes.Ldarg_0));
                    instructionList.Insert(i + 5, new CodeInstruction(OpCodes.Call, ObjectAnimationHelper.HandleNote));
                }
                if (!foundFinalPosition &&
                    instructionList[i].opcode == OpCodes.Stfld &&
                    ((FieldInfo)instructionList[i].operand).Name == "_localPosition")
                {
                    foundFinalPosition = true;
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Call, ObjectAnimationHelper.AddFinalPos));
                }
            }
            if (!foundPosition) Logger.Log("Failed to find _startPos or _endPos ldfld, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            if (!foundTime) Logger.Log("Failed to find stloc.0, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            if (!foundFinalPosition) Logger.Log("Failed to find _localPosition stfld, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
        }
    }
}
