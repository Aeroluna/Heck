using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Chroma.Lighting.EnvironmentEnhancement;
using HarmonyLib;
using Heck;
using SiraUtil.Affinity;
using UnityEngine;

namespace Chroma.HarmonyPatches.EnvironmentComponent
{
    internal class ParametricBoxControllerTransformOverride : IAffinity
    {
        private static readonly MethodInfo _localScaleSetter = AccessTools.PropertySetter(typeof(Transform), nameof(Transform.localScale));
        private static readonly MethodInfo _localPositionSetter = AccessTools.PropertySetter(typeof(Transform), nameof(Transform.localPosition));

        private readonly CodeInstruction _getTransformScale;
        private readonly CodeInstruction _getTransformPosition;
        private readonly ParametricBoxControllerParameters _parameters;

        private ParametricBoxControllerTransformOverride(ParametricBoxControllerParameters parameters)
        {
            _parameters = parameters;
            _getTransformScale = InstanceTranspilers.EmitInstanceDelegate<Func<Vector3, ParametricBoxController, Vector3>>(GetTransformScale);
            _getTransformPosition = InstanceTranspilers.EmitInstanceDelegate<Func<Vector3, ParametricBoxController, Vector3>>(GetTransformPosition);
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(ParametricBoxController), nameof(ParametricBoxController.Refresh))]
        private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _localScaleSetter))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    _getTransformScale)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _localPositionSetter))
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    _getTransformPosition)
                .InstructionEnumeration();
        }

        private Vector3 GetTransformScale(Vector3 @default, ParametricBoxController parametricBoxController)
        {
            if (_parameters.TransformParameters.TryGetValue(parametricBoxController, out ParametricBoxControllerParameters parameters) && parameters.Scale.HasValue)
            {
                return parameters.Scale.Value;
            }

            return @default;
        }

        private Vector3 GetTransformPosition(Vector3 @default, ParametricBoxController parametricBoxController)
        {
            if (_parameters.TransformParameters.TryGetValue(parametricBoxController, out ParametricBoxControllerParameters parameters) && parameters.Position.HasValue)
            {
                return parameters.Position.Value;
            }

            return @default;
        }
    }
}
