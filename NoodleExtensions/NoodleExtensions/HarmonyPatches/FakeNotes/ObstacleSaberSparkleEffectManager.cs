namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;

    [NoodlePatch(typeof(ObstacleSaberSparkleEffectManager))]
    [NoodlePatch("Update")]
    internal static class ObstacleSaberSparkleEffectManagerUpdate
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundLabel = false;
            bool foundBounds = false;
            Label? label = null;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundLabel &&
                    instructionList[i].operand is Label operandLabel &&
                    instructionList[i].operand.GetHashCode() == 0)
                {
                    foundLabel = true;

                    label = operandLabel;
                }

                if (!foundBounds &&
                    label.HasValue &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "get_bounds")
                {
                    foundBounds = true;

                    instructionList.Insert(i - 1, new CodeInstruction(OpCodes.Ldloc_1));
                    instructionList.Insert(i, new CodeInstruction(OpCodes.Call, FakeNoteHelper._boundsNullCheck));
                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Brtrue_S, label.Value));
                }
            }

            if (!foundLabel)
            {
                NoodleLogger.Log("Failed to find br to IL_0136!", IPA.Logging.Logger.Level.Error);
            }

            if (!foundBounds)
            {
                NoodleLogger.Log("Failed to find callvirt to get_bounds!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }
    }
}
