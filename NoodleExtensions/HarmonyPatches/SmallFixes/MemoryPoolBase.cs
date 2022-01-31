using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using ModestTree;
using Zenject;

namespace NoodleExtensions.HarmonyPatches.SmallFixes
{
    [HeckPatch(PatchType.Features)]
    internal static class MemoryPoolBaseSkipDespawnAssert
    {
        private static readonly MethodInfo _assertThat = AccessTools.Method(typeof(Assert), nameof(Assert.That), new[] { typeof(bool), typeof(string), typeof(object) });

        // This stupid assert runs a Contains() which is laggy when spawning an insane amount of walls
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(MemoryPoolBase<ObstacleController>), "Despawn")]
        [HarmonyPatch(typeof(MemoryPoolBase<GameNoteController>), "Despawn")]
        [HarmonyPatch(typeof(MemoryPoolBase<BombNoteController>), "Despawn")]
        private static IEnumerable<CodeInstruction> YeetAssertTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _assertThat))
                .RemoveInstructionsWithOffsets(-9, 0)
                .InstructionEnumeration();
        }
    }
}
