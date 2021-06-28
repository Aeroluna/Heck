namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;
    using static NoodleExtensions.NoodleObjectDataManager;

    // TODO: find out what actually causes obstacle flickering
    [HeckPatch(typeof(BeatmapObjectManager))]
    [HeckPatch("SpawnObstacle")]
    internal static class BeatmapObjectManagerSpawnObstacle
    {
        private static readonly MethodInfo _getHiddenForType = AccessTools.Method(typeof(BeatmapObjectManagerSpawnObstacle), nameof(GetHiddenForType));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundHide = false;
            int instructrionListCount = instructionList.Count;
            for (int i = 0; i < instructrionListCount; i++)
            {
                if (!foundHide &&
                       instructionList[i].opcode == OpCodes.Call &&
                       ((MethodInfo)instructionList[i].operand).Name == "get_spawnHidden")
                {
                    foundHide = true;

                    instructionList[i].operand = _getHiddenForType;
                }
            }

            if (!foundHide)
            {
                Plugin.Logger.Log("Failed to find call to get_spawnHidden!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static void Postfix(ObstacleController __result)
        {
            NoodleObstacleData? noodleData = TryGetObjectData<NoodleObstacleData>(__result.obstacleData);
            if (noodleData != null)
            {
                noodleData.DoUnhide = true;
            }
        }

        private static bool GetHiddenForType(BeatmapObjectManager beatmapObjectManager)
        {
            if (beatmapObjectManager is BasicBeatmapObjectManager)
            {
                return true;
            }

            return beatmapObjectManager.spawnHidden;
        }
    }
}
