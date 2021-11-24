using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;

namespace Heck.HarmonyPatches
{
    [HarmonyPatch(typeof(MirroredObstacleController))]
    [HarmonyPatch("Mirror")]
    internal static class MirroredObstacleControllerAwake
    {
        private static readonly MethodInfo _removeListeners = AccessTools.Method(typeof(MirroredObstacleController), nameof(MirroredObstacleController.RemoveListeners));

        private static readonly FieldInfo _followedObstacleField = AccessTools.Field(typeof(MirroredObstacleController), "_followedObstacle");

        // Looks like you forgot to fill _followedObstacle beat games, i got you covered!
        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _removeListeners))
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Stfld, _followedObstacleField))
                .InstructionEnumeration();
        }
    }
}
