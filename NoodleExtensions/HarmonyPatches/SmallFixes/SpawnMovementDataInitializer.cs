using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes
{
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(BeatmapObjectSpawnController))]
    internal class SpawnMovementDataInitializer
    {
        // Moves initializition of BeatmapObjectSpawnMovementData from BeatmapObjectSpawnController.Start to the Zenject intializtion phase
        private SpawnMovementDataInitializer(BeatmapObjectSpawnController.InitData initData, IBeatmapObjectSpawnController spawnController)
        {
            spawnController.beatmapObjectSpawnMovementData.Init(
                initData.noteLinesCount,
                initData.noteJumpMovementSpeed,
                initData.beatsPerMinute,
                initData.noteJumpValueType,
                initData.noteJumpValue,
                initData.jumpOffsetY,
                Vector3.right,
                Vector3.forward);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapObjectSpawnController.Start))]
        private static IEnumerable<CodeInstruction> RemoveInitTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldfld))
                .RemoveInstructions(23)
                .InstructionEnumeration();
        }
    }
}
