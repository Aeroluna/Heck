namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;
    using static NoodleExtensions.NoodleCustomDataManager;

    // TODO: find out what actually causes obstacle flickering
    [HeckPatch(typeof(BeatmapObjectManager))]
    [HeckPatch("SpawnObstacle")]
    internal static class BeatmapObjectManagerSpawnObstacle
    {
        private static readonly MethodInfo _spawnhiddenGetter = AccessTools.PropertyGetter(typeof(BeatmapObjectManager), nameof(BeatmapObjectManager.spawnHidden));

        private static readonly MethodInfo _getHiddenForType = AccessTools.Method(typeof(BeatmapObjectManagerSpawnObstacle), nameof(GetHiddenForType));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _spawnhiddenGetter))
                .SetOperandAndAdvance(_getHiddenForType)
                .InstructionEnumeration();
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
