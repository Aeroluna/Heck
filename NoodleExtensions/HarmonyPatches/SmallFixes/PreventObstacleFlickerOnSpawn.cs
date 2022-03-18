using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using SiraUtil.Affinity;
using Zenject;

namespace NoodleExtensions.HarmonyPatches.SmallFixes
{
    // TODO: find out what actually causes obstacle flickering
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(BeatmapObjectManager))]
    internal class PreventObstacleFlickerOnSpawn : IAffinity
    {
        private static readonly MethodInfo _spawnhiddenGetter = AccessTools.PropertyGetter(typeof(BeatmapObjectManager), nameof(BeatmapObjectManager.spawnHidden));
        private static readonly MethodInfo _getHiddenForType = AccessTools.Method(typeof(PreventObstacleFlickerOnSpawn), nameof(GetHiddenForType));

        private readonly CustomData _customData;

        private PreventObstacleFlickerOnSpawn([Inject(Id = NoodleController.ID)] CustomData customData)
        {
            _customData = customData;
        }

        private static bool GetHiddenForType(BeatmapObjectManager beatmapObjectManager)
        {
            return beatmapObjectManager is BasicBeatmapObjectManager || beatmapObjectManager.spawnHidden;
        }

        [HarmonyTranspiler]
        [HarmonyPatch("AddSpawnedObstacleController")]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _spawnhiddenGetter))
                .SetOperandAndAdvance(_getHiddenForType)
                .InstructionEnumeration();
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectManager), "AddSpawnedObstacleController")]
        private void SetUnhideFlag(ObstacleController obstacleController)
        {
            if (_customData.Resolve(obstacleController.obstacleData, out NoodleObstacleData? noodleData))
            {
                noodleData.DoUnhide = true;
            }
        }
    }
}
