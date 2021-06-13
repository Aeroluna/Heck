namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;

    [HeckPatch(typeof(PlayerHeadAndObstacleInteraction))]
    [HeckPatch("GetObstaclesContainingPoint")]
    internal static class PlayerHeadAndObstacleInteractionGetObstaclesContainingPoint
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundCondition = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundCondition &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "get_hasPassedAvoidedMark")
                {
                    foundCondition = true;

                    CodeInstruction[] codeInstructions = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldloc_1),
                        new CodeInstruction(OpCodes.Call, FakeNoteHelper._boundsNullCheck),
                        new CodeInstruction(OpCodes.Brtrue_S, instructionList[i + 1].operand),
                    };
                    instructionList.InsertRange(i + 2, codeInstructions);
                }
            }

            if (!foundCondition)
            {
                Plugin.Logger.Log("Failed to find brtrue.s to IL_004E!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }
    }
}
