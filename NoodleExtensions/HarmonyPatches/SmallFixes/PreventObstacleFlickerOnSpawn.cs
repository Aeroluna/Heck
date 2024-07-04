using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using Heck.Deserialize;
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

        private readonly DeserializedData _deserializedData;

        private PreventObstacleFlickerOnSpawn([Inject(Id = NoodleController.ID)] DeserializedData deserializedData)
        {
            _deserializedData = deserializedData;
        }

        private static bool GetHiddenForType(BeatmapObjectManager beatmapObjectManager)
        {
            return beatmapObjectManager is BasicBeatmapObjectManager || beatmapObjectManager.spawnHidden;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapObjectManager.AddSpawnedObstacleController))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * -- obstacleController.Hide(this.spawnHidden);
                 * ++ obstacleController.Hide(GetHiddenForType(this));
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Call, _spawnhiddenGetter))
                .SetOperandAndAdvance(_getHiddenForType)
                .InstructionEnumeration();
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(BeatmapObjectManager), "AddSpawnedObstacleController")]
        private void SetUnhideFlag(ObstacleController obstacleController)
        {
            if (_deserializedData.Resolve(obstacleController.obstacleData, out NoodleObstacleData? noodleData))
            {
                noodleData.InternalDoUnhide = true;
            }
        }
    }
}
