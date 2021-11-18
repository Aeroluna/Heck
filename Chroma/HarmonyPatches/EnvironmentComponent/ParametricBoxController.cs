namespace Chroma.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using Chroma;
    using HarmonyLib;
    using Heck;
    using UnityEngine;

    [HeckPatch(typeof(ParametricBoxController))]
    [HeckPatch("Refresh")]
    internal static class ParametricBoxControllerRefresh
    {
        private static readonly MethodInfo _localScaleSetter = AccessTools.PropertySetter(typeof(Transform), nameof(Transform.localScale));
        private static readonly MethodInfo _localPositionSetter = AccessTools.PropertySetter(typeof(Transform), nameof(Transform.localPosition));

        private static readonly MethodInfo _getTransformScale = AccessTools.Method(typeof(ParametricBoxControllerRefresh), nameof(GetTransformScale));
        private static readonly MethodInfo _getTransformPosition = AccessTools.Method(typeof(ParametricBoxControllerRefresh), nameof(GetTransformPosition));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _localScaleSetter))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, _getTransformScale))
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _localPositionSetter))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, _getTransformPosition))
                .InstructionEnumeration();
        }

        private static Vector3 GetTransformScale(Vector3 @default, ParametricBoxController parametricBoxController)
        {
            if (ParametricBoxControllerParameters.TransformParameters.TryGetValue(parametricBoxController, out ParametricBoxControllerParameters parameters) && parameters.Scale.HasValue)
            {
                return parameters.Scale.Value;
            }

            return @default;
        }

        private static Vector3 GetTransformPosition(Vector3 @default, ParametricBoxController parametricBoxController)
        {
            if (ParametricBoxControllerParameters.TransformParameters.TryGetValue(parametricBoxController, out ParametricBoxControllerParameters parameters) && parameters.Position.HasValue)
            {
                return parameters.Position.Value;
            }

            return @default;
        }
    }
}
