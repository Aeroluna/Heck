namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
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
                    instructionList[i].operand is Label &&
                    instructionList[i].operand.GetHashCode() == 1)
                {
                    foundCondition = true;

                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Ldloc_1));
                    instructionList.Insert(i + 2, new CodeInstruction(OpCodes.Call, FakeNoteHelper._boundsNullCheck));
                    instructionList.Insert(i + 3, new CodeInstruction(OpCodes.Brtrue_S, instructionList[i].operand));
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
