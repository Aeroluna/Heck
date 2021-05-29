namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;
    using Zenject;

    [HeckPatch(typeof(MemoryPoolBase<ObstacleController>))]
    [HeckPatch("Despawn")]
    internal static class MemoryPoolBaseObstacleControllerDespawn
    {
        // This stupid assert runs a Contains() which is laggy when spawning an insane amount of walls
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundAssert = false;
            int instructrionListCount = instructionList.Count;
            for (int i = 0; i < instructrionListCount; i++)
            {
                if (!foundAssert &&
                       instructionList[i].opcode == OpCodes.Call &&
                       ((MethodInfo)instructionList[i].operand).Name == "That")
                {
                    foundAssert = true;

                    instructionList.RemoveRange(i - 9, 10);
                }
            }

            if (!foundAssert)
            {
                Plugin.Logger.Log("Failed to find call to That!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }
    }
}
