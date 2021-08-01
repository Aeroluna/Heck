namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using HarmonyLib;
    using Heck;

    [HeckPatch(typeof(GameEnergyCounter))]
    [HeckPatch("LateUpdate")]
    internal static class GameEnergyCounterUpdate
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return FakeNoteHelper.ObstaclesTranspiler(instructions);
        }
    }
}
