namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;
    using UnityEngine;

    [HeckPatch(typeof(NoteCutter))]
    [HeckPatch("Cut")]
    internal static class NoteCutterCut
    {
        private static readonly FieldInfo _topPos = AccessTools.Field(typeof(BladeMovementDataElement), nameof(BladeMovementDataElement.topPos));
        private static readonly FieldInfo _bottomPos = AccessTools.Field(typeof(BladeMovementDataElement), nameof(BladeMovementDataElement.bottomPos));

        private static readonly MethodInfo _movementDataGetter = AccessTools.PropertyGetter(typeof(Saber), nameof(Saber.movementData));
        private static readonly MethodInfo _lastAddedDataGetter = AccessTools.PropertyGetter(typeof(SaberMovementData), nameof(SaberMovementData.lastAddedData));

        private static readonly MethodInfo _convertToWorld = AccessTools.Method(typeof(NoteCutterCut), nameof(NoteCutterCut.ConvertToWorld));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_3))
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloca_S, 2),
                    new CodeInstruction(OpCodes.Ldloca_S, 3),
                    new CodeInstruction(OpCodes.Call, _convertToWorld))
                .InstructionEnumeration();
        }

        private static void ConvertToWorld(Saber saber, ref Vector3 topPos, ref Vector3 bottomPos)
        {
            Transform playerTransform = saber.transform.parent.parent;
            topPos = playerTransform.TransformPoint(topPos);
            bottomPos = playerTransform.TransformPoint(bottomPos);
        }
    }
}
