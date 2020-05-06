using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapObjectCallbackController))]
    [HarmonyPatch("LateUpdate")]
    internal class BeatmapObjectCallBackControllerLateUpdate
    {
        private static readonly MethodInfo aheadTime = SymbolExtensions.GetMethodInfo(() => GetAheadTime(null, 0));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundAheadTime = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundAheadTime &&
                    instructionList[i].opcode == OpCodes.Ldfld &&
                    ((FieldInfo)instructionList[i].operand).Name == "aheadTime")
                {
                    foundAheadTime = true;

                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, aheadTime));
                    instructionList.Insert(i - 1, new CodeInstruction(OpCodes.Ldloc_3));
                }
            }
            if (!foundAheadTime) Logger.Log("Failed to find aheadTime ldfld, ping Aeroluna!", IPA.Logging.Logger.Level.Error);
            return instructionList.AsEnumerable();
        }

        private static float GetAheadTime(BeatmapObjectData beatmapObjectData, float @default)
        {
            if (beatmapObjectData is CustomObstacleData || beatmapObjectData is CustomNoteData)
            {
                dynamic dynData = ((dynamic)beatmapObjectData).customData;
                float? aheadTime = (float)Trees.at(dynData, "aheadTime");
                if (aheadTime.HasValue) return aheadTime.Value;
            }
            return @default;
        }
    }
}