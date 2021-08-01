namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;
    using ModestTree;
    using Zenject;

    [HeckPatch(typeof(MemoryPoolBase<ObstacleController>))]
    [HeckPatch("Despawn")]
    internal static class MemoryPoolBaseObstacleControllerDespawn
    {
        private static readonly MethodInfo _assertThat = AccessTools.Method(typeof(Assert), nameof(Assert.That), new System.Type[] { typeof(bool), typeof(string), typeof(object) });

        // This stupid assert runs a Contains() which is laggy when spawning an insane amount of walls
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _assertThat))
                .RemoveInstructionsWithOffsets(-9, 0)
                .InstructionEnumeration();
        }
    }
}
