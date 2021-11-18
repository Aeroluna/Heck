namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;

    [HeckPatch(typeof(ObstacleSaberSparkleEffectManager))]
    [HeckPatch("Update")]
    internal static class ObstacleSaberSparkleEffectManagerUpdate
    {
        private static readonly MethodInfo _currentGetter = AccessTools.PropertyGetter(typeof(List<ObstacleController>.Enumerator), nameof(List<ObstacleController>.Enumerator.Current));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Br));

            object label = codeMatcher.Operand;

            return codeMatcher
                .MatchForward(false, new CodeMatch(OpCodes.Call, _currentGetter))
                .Advance(2)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, FakeNoteHelper._boundsNullCheck),
                    new CodeInstruction(OpCodes.Brtrue_S, label))
                .InstructionEnumeration();
        }
    }
}
