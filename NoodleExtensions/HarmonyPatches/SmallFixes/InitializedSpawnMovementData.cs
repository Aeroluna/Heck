using System.Collections.Generic;
using HarmonyLib;
using Heck;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes
{
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(BeatmapObjectSpawnController))]
    internal class InitializedSpawnMovementData
    {
        // Moves initializition of BeatmapObjectSpawnMovementData from BeatmapObjectSpawnController.Start to the Zenject intializtion phase
        private InitializedSpawnMovementData(
            BeatmapObjectSpawnController.InitData initData,
            IJumpOffsetYProvider jumpOffsetYProvider,
            IBeatmapObjectSpawnController spawnController)
        {
            spawnController.beatmapObjectSpawnMovementData.Init(
                initData.noteLinesCount,
                initData.noteJumpMovementSpeed,
                initData.beatsPerMinute,
                initData.noteJumpValueType,
                initData.noteJumpValue,
                jumpOffsetYProvider,
                Vector3.right,
                Vector3.forward);

            MovementData = spawnController.beatmapObjectSpawnMovementData;
        }

        internal BeatmapObjectSpawnMovementData MovementData { get; }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapObjectSpawnController.Start))]
        private static IEnumerable<CodeInstruction> RemoveInitTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .Start()
                .RemoveInstructions(22)
                .InstructionEnumeration();
        }
    }
}
