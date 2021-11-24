using System.Collections.Generic;
using HarmonyLib;
using Heck;
using JetBrains.Annotations;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HeckPatch(typeof(GameEnergyCounter))]
    [HeckPatch("LateUpdate")]
    internal static class GameEnergyCounterUpdate
    {
        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return FakeNoteHelper.ObstaclesTranspiler(instructions);
        }
    }
}
