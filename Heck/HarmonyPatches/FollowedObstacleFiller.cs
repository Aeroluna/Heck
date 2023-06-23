using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Heck.HarmonyPatches
{
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(MirroredObstacleController))]
    internal static class FollowedObstacleFiller
    {
        private static readonly MethodInfo _removeListeners = AccessTools.Method(typeof(MirroredObstacleController), nameof(MirroredObstacleController.RemoveListeners));

        private static readonly FieldInfo _followedObstacleField = AccessTools.Field(typeof(MirroredObstacleController), "_followedObstacle");

        // Looks like you forgot to fill _followedObstacle beat games, i got you covered!
        // This is a transpiler so that the field gets filled before UpdatePositionAndRotation and InvokeDidInitEvent
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(MirroredObstacleController.Mirror))]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * this.RemoveListeners();
                 * ++ _followedObstacle = obstacleController;
                 */
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
