namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;

    [HarmonyPatch(typeof(MirroredObstacleController))]
    [HarmonyPatch("Mirror")]
    internal static class MirroredObstacleControllerAwake
    {
        private static readonly FieldInfo _followedObstacleField = AccessTools.Field(typeof(MirroredObstacleController), "_followedObstacle");

        // Looks like you forgot to fill _followedObstacle beat games, i got you covered!
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundRemoveListeners = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundRemoveListeners &&
                    instructionList[i].opcode == OpCodes.Call &&
                    ((MethodInfo)instructionList[i].operand).Name == "RemoveListeners")
                {
                    foundRemoveListeners = true;
                    CodeInstruction[] codeInstructions = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Stfld, _followedObstacleField),
                    };
                    instructionList.InsertRange(i + 1, codeInstructions);
                }
            }

            if (!foundRemoveListeners)
            {
                Plugin.Logger.Log("Failed to find call to RemoveListeners!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }
    }
}
