namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;

    [HeckPatch(typeof(BeatmapObjectExecutionRatingsRecorder))]
    [HeckPatch("Update")]
    internal static class BeatmapObjectExecutionRatingsRecorderUpdate
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundIntersectingObstacles = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundIntersectingObstacles &&
                    instructionList[i].opcode == OpCodes.Callvirt &&
                    ((MethodInfo)instructionList[i].operand).Name == "get_intersectingObstacles")
                {
                    foundIntersectingObstacles = true;

                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, FakeNoteHelper._obstacleFakeCheck));
                }
            }

            if (!foundIntersectingObstacles)
            {
                Plugin.Logger.Log("Failed to find callvirt to get_intersectingObstacles!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }
    }
}
